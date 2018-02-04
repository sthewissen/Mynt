using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Mynt.Core.Api;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Managers;
using Mynt.Core.Models;

namespace Mynt.Core.TradeManagers
{
    public class GenericTradeManager
    {
        private readonly IExchangeApi _api;
        private readonly INotificationManager _notification;
        private readonly ITradingStrategy _strategy;
        private readonly Action<string> _log;
        private CloudTable _orderTable;
        private CloudTable _traderTable;

        public GenericTradeManager(IExchangeApi api, ITradingStrategy strat, INotificationManager notificationManager, Action<string> log)
        {
            _api = api;
            _strategy = strat;
            _log = log;
            _notification = notificationManager;
        }

        /// <summary>
        /// Checks if new trades can be started.
        /// </summary>
        /// <returns></returns>
        public async Task CheckForBuySignals()
        {
            // Get our current trades.
            _orderTable = await ConnectionManager.GetTableConnection(Constants.OrderTableName, Constants.IsDryRunning);
            _traderTable = await ConnectionManager.GetTableConnection(Constants.TraderTableName, Constants.IsDryRunning);

            var activeTrades = _orderTable.CreateQuery<Trade>().Where(x => x.IsOpen).ToList();
            var currentTraders = _traderTable.CreateQuery<Trader>().ToList();

            // Create our trader records if they don't exist yet.
            if (currentTraders.Count == 0) await CreateTradersIfNoneExist();

            // Create a batch that we can use to update our table.
            var orderBatch = new TableBatchOperation();
            var traderBatch = new TableBatchOperation();

            // Get a list of traders that are currently standing there doing nothing...
            var freeTraders = _traderTable.CreateQuery<Trader>().Where(x => !x.IsBusy).ToList();

            if (freeTraders.Count > 0)
            {
                // We have slots open!
                var trades = await FindTrades(activeTrades);

                if (trades.Count > 0)
                {
                    // Depending on what we have more of we create trades.
                    var loopCount = freeTraders.Count >= trades.Count ? trades.Count : freeTraders.Count;

                    // Only create trades for our free traders
                    for (int i = 0; i < loopCount; i++)
                    {
                        // Get our Bitcoin balance from the exchange
                        var currentBtcBalance = await _api.GetBalance("BTC");

                        // Do we even have enough funds to invest?
                        if (currentBtcBalance.Available < freeTraders[i].CurrentBalance)
                            throw new Exception("Insufficient BTC funds to perform a trade.");

                        var order = await CreateBuyOrder(freeTraders[i], trades[i]);

                        // We found a trade and have set it all up!
                        if (order != null)
                        {
                            // Send a notification that we found something suitable
                            _log($"New trade signal {order.Market}...");
                            await SendNotification($"Buying {order.Market} at {order.OpenRate:0.0000000 BTC} ({order.Quantity:0.0000} units)");

                            // Update the trader to busy
                            freeTraders[i].LastUpdated = DateTime.UtcNow;
                            freeTraders[i].IsBusy = true;
                            traderBatch.Add(TableOperation.Replace(freeTraders[i]));

                            // Create the trade record as well
                            orderBatch.Add(TableOperation.Insert(order));
                        }
                    }
                }

                if (traderBatch.Count > 0) await _traderTable.ExecuteBatchAsync(traderBatch);
                if (orderBatch.Count > 0) await _orderTable.ExecuteBatchAsync(orderBatch);
            }
        }

        public async Task CheckForSellSignals()
        {
            // Get our current trades.
            _orderTable = await ConnectionManager.GetTableConnection(Constants.OrderTableName, Constants.IsDryRunning);
            _traderTable = await ConnectionManager.GetTableConnection(Constants.TraderTableName, Constants.IsDryRunning);

            var activeTrades = _orderTable.CreateQuery<Trade>().Where(x => x.IsOpen).ToList();
            var busyTraders = _traderTable.CreateQuery<Trader>().Where(x => x.IsBusy).ToList();

            // First we update our open buy orders by checking if they're filled.
            activeTrades = await UpdateOpenBuyOrders(activeTrades);

            // Secondly we check if currently selling trades can be marked as sold if they're filled.
            activeTrades = await UpdateOpenSellOrders(busyTraders, activeTrades);

            // Third, our current trades need to be checked if one of these has hit its sell targets...
            await CheckForSellConditions(activeTrades);
        }

        /// <summary>
        /// Creates trader objects that run in their own little bubble.
        /// </summary>
        /// <returns></returns>
        private async Task CreateTradersIfNoneExist()
        {
            var tableBatch = new TableBatchOperation();

            for (var i = 0; i < Constants.MaxNumberOfConcurrentTrades; i++)
            {
                var newTrader = new Trader()
                {
                    CurrentBalance = Constants.AmountOfBtcToInvestPerTrader,
                    IsBusy = false,
                    LastUpdated = DateTime.UtcNow,
                    StakeAmount = Constants.AmountOfBtcToInvestPerTrader,
                    RowKey = Guid.NewGuid().ToString().Replace("-", string.Empty),
                    PartitionKey = "TRADER"
                };

                tableBatch.Add(TableOperation.Insert(newTrader));
            }

            // Add our trader records
            if (tableBatch.Count > 0) await _traderTable.ExecuteBatchAsync(tableBatch);
        }

        #region BUY side

        /// <summary>
        /// Updates the buy orders by checking with the exchange what status they are currently.
        /// </summary>
        /// <param name="activeTrades"></param>
        /// <returns></returns>
        private async Task<List<Trade>> UpdateOpenBuyOrders(List<Trade> activeTrades)
        {
            // There are trades that have an open order ID set & no sell order id set
            // that means its a buy trade that is waiting to get bought. See if we can update that first.
            var orderBatch = new TableBatchOperation();

            foreach (var trade in activeTrades.Where(x => x.OpenOrderId != null && x.SellOrderId == null))
            {
                var exchangeOrder = await _api.GetOrder(trade.BuyOrderId, trade.Market);

                // if this order is filled, we can update our database.
                if (exchangeOrder.Status == OrderStatus.Filled)
                {
                    trade.OpenOrderId = null;
                    trade.StakeAmount = exchangeOrder.OriginalQuantity * exchangeOrder.Price;
                    trade.Quantity = exchangeOrder.OriginalQuantity;
                    trade.OpenRate = exchangeOrder.Price;
                    trade.OpenDate = exchangeOrder.Time;

                    orderBatch.Add(TableOperation.Replace(trade));
                }
            }

            if (orderBatch.Count > 0) await _orderTable.ExecuteBatchAsync(orderBatch);

            return activeTrades;
        }

        /// <summary>
        /// Checks the implemented trading indicator(s),
        /// if one pair triggers the buy signal a new trade record gets created.
        /// </summary>
        /// <param name="trades"></param>
        /// <returns></returns>
        private async Task<List<string>> FindTrades(List<Trade> trades)
        {
            // Retrieve our current markets
            var markets = await _api.GetMarketSummaries();
            var pairs = new List<string>();

            // Check if there are markets matching our volume.
            markets = markets.Where(x => (x.BaseVolume > Constants.MinimumAmountOfVolume || Constants.AlwaysTradeList.Contains(x.CurrencyPair.BaseCurrency)) &&
            x.CurrencyPair.QuoteCurrency.ToUpper() == "BTC").ToList();

            // Remove existing trades from the list to check.
            foreach (var trade in trades)
                markets.RemoveAll(x => x.MarketName == trade.Market);

            // Remove items that are on our blacklist.
            foreach (var market in Constants.MarketBlackList)
                markets.RemoveAll(x => x.CurrencyPair.BaseCurrency == market);

            // Prioritize markets with high volume.
            foreach (var market in markets.Distinct().OrderByDescending(x => x.BaseVolume).ToList())
            {
                if (await GetBuySignal(market.MarketName))
                {
                    // A match was made, buy that please!
                    pairs.Add(market.MarketName);
                }
            }

            return pairs;
        }

        /// <summary>
        /// Creates a buy order on the exchange.
        /// </summary>
        /// <param name="freeTrader">The trader placing the order</param>
        /// <param name="pair">The pair we're buying</param>
        /// <returns></returns>
        private async Task<Trade> CreateBuyOrder(Trader freeTrader, string pair)
        {
            // Take the amount to invest per trader OR the current balance for this trader.
            var btcToSpend = freeTrader.CurrentBalance > Constants.AmountOfBtcToInvestPerTrader
                ? Constants.AmountOfBtcToInvestPerTrader
                : freeTrader.CurrentBalance;

            // The amount here is an indication and will probably not be precisely what you get.
            var openRate = GetTargetBid(await _api.GetTicker(pair));
            var amount = btcToSpend / openRate;
            var amountYouGet = (btcToSpend * (1 - Constants.TransactionFeePercentage)) / openRate;

            // Get the order ID, this is the most important because we need this to check
            // up on our trade. We update the data below later when the final data is present.
            var orderId = await _api.Buy(pair, amount, openRate);

            return new Trade()
            {
                TraderId = freeTrader.RowKey,
                Market = pair,
                StakeAmount = btcToSpend,
                OpenRate = openRate,
                OpenDate = DateTime.UtcNow,
                Quantity = amountYouGet,
                OpenOrderId = orderId,
                BuyOrderId = orderId,
                IsOpen = true,
                StrategyUsed = _strategy.Name,
                PartitionKey = "TRADE",
                SellType = SellType.None,
                RowKey = $"MNT{(DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks):d19}"
            };
        }

        /// <summary>
        /// Calculates a buy signal based on several technical analysis indicators.
        /// </summary>
        /// <param name="market">The market we're going to check against.</param>
        /// <returns></returns>
        private async Task<bool> GetBuySignal(string market)
        {
            try
            {
                _log($"Checking market {market}...");

                var advice = await GetAdvice(market);

                // If the last signal was a 1, we buy!
                return advice != null && advice.TradeAdvice == TradeAdvice.Buy;
            }
            catch (Exception)
            {
                // Couldn't get a buy signal for this market, no problem. Let's skip it.
                _log($"Couldn't get buy signal for {market}...");
                return false;
            }
        }

        /// <summary>
        /// Calculates bid target between current ask price and last price.
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        private double GetTargetBid(Ticker tick)
        {
            // If the ask is below the last, we can get it on the cheap.
            if (tick.Ask < tick.Last) return tick.Ask;

            return tick.Ask + Constants.AskLastBalance * (tick.Last - tick.Ask);
        }

        /// <summary>
        /// Retrieves an advice (e.g. buy, sell, hold) for the given market.
        /// </summary>
        /// <param name="tradeMarket"></param>
        /// <returns></returns>
        private async Task<ITradeAdvice> GetAdvice(string tradeMarket)
        {
            var minimumDate = DateTime.UtcNow.AddHours(-120);
            var candleDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.UtcNow.Day, DateTime.Now.Hour, 0, 0, 0);
            var candles = await _api.GetTickerHistory(tradeMarket, minimumDate, Core.Models.Period.Hour);

            // We eliminate all candles that aren't needed and the last one (if it's the current running candle).
            candles = candles.Where(x => x.Timestamp > minimumDate && x.Timestamp < candleDate).ToList();

            // Get the date for the last candle we have left.
            var signalDate = candles[candles.Count - 1].Timestamp;

            // This is an outdated candle...
            if (signalDate < DateTime.UtcNow.AddMinutes(-120))
                return null;

            // This calculates an advice for the next timestamp.
            var advice = _strategy.Forecast(candles);

            return advice;
        }

        #endregion

        #region SELL side

        /// <summary>
        /// Checks the current active trades if they need to be sold.
        /// </summary>
        /// <param name="activeTrades"></param>
        /// <returns></returns>
        private async Task CheckForSellConditions(List<Trade> activeTrades)
        {
            // There are trades that have an open order ID set & no sell order id set
            // that means its a buy trade that is waiting to get bought. See if we can update that first.
            var orderBatch = new TableBatchOperation();
            
            foreach (var trade in activeTrades.Where(x => x.OpenOrderId == null && x.IsOpen))
            {
                // These are trades that are not being bought or sold at the moment so these need to be checked for sell conditions.
                var ticker = await _api.GetTicker(trade.Market);
                var sellType = ShouldSell(trade, ticker.Bid, DateTime.UtcNow);

                if (sellType != SellType.None)
                {
                    var orderId = await _api.Sell(trade.Market, trade.Quantity, ticker.Bid);

                    trade.CloseRate = ticker.Bid;
                    trade.OpenOrderId = orderId;
                    trade.SellOrderId = orderId;
                    trade.SellType = sellType;

                    orderBatch.Add(TableOperation.Replace(trade));
                }
            }
            
            if (orderBatch.Count > 0) await _orderTable.ExecuteBatchAsync(orderBatch);
        }

        /// <summary>
        /// Based on earlier trade and current price and configuration, decides whether bot should sell.
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="currentRateBid"></param>
        /// <param name="utcNow"></param>
        /// <returns>True if bot should sell at current rate.</returns>
        private SellType ShouldSell(Trade trade, double currentRateBid, DateTime utcNow)
        {
            var currentProfit = (currentRateBid - trade.OpenRate) / trade.OpenRate;

            _log($"Should sell {trade.Market}? Profit: {(currentProfit * 100):0.00}%...");

            // Let's not do a stoploss for now...
            if (currentProfit < Constants.StopLossPercentage)
            {
                _log($"Stop loss hit: {Constants.StopLossPercentage}");
                return SellType.StopLoss;
            }

            //if (currentProfit < trade.StopLossAnchor)
            //    return SellType.StopLossAnchor;

            //// Set a stop loss anchor to minimize losses.
            //foreach (var item in Constants.StopLossAnchors)
            //{
            //    if (currentProfit > item)
            //        trade.StopLossAnchor = item - 0.01;
            //}

            // Check if time matches and current rate is above threshold
            foreach (var item in Constants.ReturnOnInvestment)
            {
                var timeDiff = (utcNow - trade.OpenDate).TotalSeconds / 60;

                if (timeDiff > item.Duration && currentProfit > item.Profit)
                {
                    _log($"Timer hit: {timeDiff} mins, profit {item.Profit:0.00}%");
                    return SellType.Timed;
                }
            }

            return SellType.None;
        }

        /// <summary>
        /// Updates the sell orders by checking with the exchange what status they are currently.
        /// </summary>
        /// <returns></returns>
        private async Task<List<Trade>> UpdateOpenSellOrders(List<Trader> busyTraders, List<Trade> activeTrades)
        {
            // There are trades that have an open order ID set & sell order id set
            // that means its a sell trade that is waiting to get sold. See if we can update that first.
            var orderBatch = new TableBatchOperation();
            var traderBatch = new TableBatchOperation();

            foreach (var order in activeTrades.Where(x => x.OpenOrderId != null && x.SellOrderId != null))
            {
                var exchangeOrder = await _api.GetOrder(order.SellOrderId, order.Market);

                // if this order is filled, we can update our database.
                if (exchangeOrder.Status == OrderStatus.Filled)
                {
                    order.OpenOrderId = null;
                    order.IsOpen = false;
                    order.CloseDate = exchangeOrder.Time;

                    order.CloseProfit = (exchangeOrder.Price * exchangeOrder.OriginalQuantity) - order.StakeAmount;
                    order.CloseProfitPercentage = ((exchangeOrder.Price * exchangeOrder.OriginalQuantity) - order.StakeAmount) / order.StakeAmount * 100;

                    // Retrieve the trader respsonsible for this trade
                    var trader = busyTraders.FirstOrDefault(x => x.RowKey == order.TraderId);

                    if (trader != null)
                    {
                        trader.IsBusy = false;
                        trader.CurrentBalance += order.CloseProfit.Value;
                        trader.LastUpdated = DateTime.UtcNow;
                    }

                    traderBatch.Add(TableOperation.Replace(trader));
                    orderBatch.Add(TableOperation.Replace(order));
                }
            }

            if (traderBatch.Count > 0) await _traderTable.ExecuteBatchAsync(traderBatch);
            if (orderBatch.Count > 0) await _orderTable.ExecuteBatchAsync(orderBatch);

            return activeTrades;
        }

        #endregion

        private async Task SendNotification(string message)
        {
            if (_notification != null)
            {
                await _notification.SendNotification(message);
            }
        }
    }
}