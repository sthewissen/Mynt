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
    public class GenericTradeManager : ITradeManager
    {
        private readonly IExchangeApi _api;
        private readonly INotificationManager _notification;
        private readonly ITradingStrategy _strategy;
        private readonly Action<string> _log;
        private Balance _totalBalance;
        private Balance _dayBalance;
        private bool _totalBalanceExists;
        private bool _dayBalanceExists;
        private double _oldDayBalance;
        private double _oldTotalBalance;

        public GenericTradeManager(IExchangeApi api, ITradingStrategy strat, INotificationManager notificationManager, Action<string> log)
        {
            _api = api;
            _strategy = strat;
            _log = log;
            _notification = notificationManager;
        }

        /// <summary>
        /// Queries the persistence layer for open trades and 
        /// handles them, otherwise a new trade is created.
        /// </summary>
        /// <returns></returns>
        public async Task Process()
        {
            // Get our current trades.
            var tradeTable = await ConnectionManager.GetTableConnection(Constants.OrderTableName, Constants.IsDryRunning);
            var balanceTable = await ConnectionManager.GetTableConnection(Constants.BalanceTableName,
                Constants.IsDryRunning);
            var activeTrades = tradeTable.CreateQuery<Trade>().Where(x => x.IsOpen).ToList();

            // Create two batches that we can use to update our tables.
            var batch = new TableBatchOperation();
            var balanceBatch = new TableBatchOperation();

            // Can't use FirstOrDefault directly because Linq for Table Storage doesn't support it.
            _totalBalance = balanceTable.CreateQuery<Balance>().Where(x => x.RowKey == "TOTAL").FirstOrDefault();
            _dayBalance =
                balanceTable.CreateQuery<Balance>()
                    .Where(x => x.RowKey == DateTime.UtcNow.ToString("yyyyMMdd"))
                    .FirstOrDefault();

            // Create both the balances if they don't exist yet.
            CreateBalancesIfNotExists(balanceBatch);

            // Handle our active trades.
            foreach (var trade in activeTrades)
            {
                var orders = await _api.GetOpenOrders(trade.Market);

                if (orders.Any(x => x.OrderUuid.ToString() == trade.OpenOrderId))
                {
                    // There's already an open order for this trade.
                    // This means we're still buying it.
                    _log($"Already an open order for trade {trade.OpenOrderId}");
                }
                else
                {
                    trade.OpenOrderId = null;

                    // No open order with the order ID of the trade.
                    // Check if this trade can be closed
                    if (!await CloseTradeIfFulfilled(trade))
                    {
                        // Check if we can sell our current pair
                        await HandleTrade(trade);
                    }

                    batch.Add(TableOperation.Replace(trade));
                }
            }

            // If we have less active trades than we can handle, find a new one.
            while (activeTrades.Count < Constants.MaxNumberOfConcurrentTrades)
            {
                var trade = await StartTrade(activeTrades, batch);

                if (trade != null)
                {
                    // Add this to activeTrades so we don't trigger the same.
                    activeTrades.Add(trade);
                    batch.Add(TableOperation.Insert(trade));
                }
                else
                {
                    // No more trade to be found, kill it.
                    break;
                }
            }

            _log($"Currently handling {activeTrades.Count} trades.");

            // If these actually changed make a roundtrip to the server to set them.
            if (_dayBalanceExists && _oldDayBalance != _dayBalance.Profit) balanceBatch.Add(TableOperation.Replace(_dayBalance));
            if (_totalBalanceExists && _oldTotalBalance != _totalBalance.Profit) balanceBatch.Add(TableOperation.Replace(_totalBalance));

            if (batch.Count > 0) tradeTable.ExecuteBatch(batch);
            if (balanceBatch.Count > 0) balanceTable.ExecuteBatch(balanceBatch);
        }

        /// <summary>
        /// Directly triggers a sell.
        /// </summary>
        /// <param name="trade"></param>
        /// <returns></returns>
        public async Task DirectSell(Trade trade)
        {
            var currentRate = await _api.GetTicker(trade.Market);
            await ExecuteSell(trade, currentRate.Bid);
        }

        /// <summary>
        /// Creates our total and daily balance records in the Azure Table Storage.
        /// </summary>
        /// <param name="balanceBatch"></param>
        private void CreateBalancesIfNotExists(TableBatchOperation balanceBatch)
        {
            _totalBalanceExists = _totalBalance != null;
            _dayBalanceExists = _dayBalance != null;

            if (_totalBalance == null)
            {
                _totalBalance = new Balance()
                {
                    LastUpdated = DateTime.UtcNow,
                    PartitionKey = "BALANCE",
                    RowKey = "TOTAL",
                    TotalBalance = Constants.MaxNumberOfConcurrentTrades * Constants.AmountOfBtcToInvestPerTrader,
                    Profit = 0
                };

                balanceBatch.Add(TableOperation.Insert(_totalBalance));
            }
            else
            {
                _oldTotalBalance = _totalBalance.Profit;
            }

            if (_dayBalance == null)
            {
                _dayBalance = new Balance()
                {
                    BalanceDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day),
                    LastUpdated = DateTime.UtcNow,
                    PartitionKey = "BALANCE",
                    RowKey = DateTime.UtcNow.ToString("yyyyMMdd"),
                    Profit = 0
                };

                balanceBatch.Add(TableOperation.Insert(_dayBalance));
            }
            else
            {
                _oldDayBalance = _dayBalance.Profit;
            }
        }

        /// <summary>
        /// Starts finding an actual strade.
        /// </summary>
        /// <param name="activeTrades"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        private async Task<Trade> StartTrade(List<Trade> activeTrades, TableBatchOperation batch)
        {
            try
            {
                // We have slots open!
                var trade = await FindTrade(activeTrades, Constants.AmountOfBtcToInvestPerTrader);

                if (trade != null)
                {
                    _log($"New trade signal {trade.Market}...");
                    await SendNotification($"Buying {trade.Market} at {trade.OpenRate:0.0000000 BTC} ({trade.Quantity:0.0000} units)");
                    return trade;
                }

                // Else we didn't have a buy signal
                _log("No trade signals found...");
                return null;
            }
            catch (Exception ex)
            {
                // Trade couldn't be created.
                _log($"{ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks the implemented trading indicator(s),
        /// if one pair triggers the buy signal a new trade record gets created.
        /// </summary>
        /// <param name="trades"></param>
        /// <param name="amountOfBtcToInvestPerTrader"></param>
        /// <returns></returns>
        private async Task<Trade> FindTrade(List<Trade> trades, double amountOfBtcToInvestPerTrader)
        {
            // Get our Bitcoin balance from the exchange
            var currentBtcBalance = await _api.GetBalance("BTC");

            // Do we even have enough funds to invest?
            if (currentBtcBalance < Constants.AmountOfBtcToInvestPerTrader)
                throw new Exception("Insufficient BTC funds to perform a trade.");

            // Retrieve our current markets
            var markets = await _api.GetMarketSummaries();

            // Check if there are markets matching our volume.
            markets = markets.Where(x => (x.BaseVolume > Constants.MinimumAmountOfVolume || Constants.AlwaysTradeList.Contains(x.MarketName)) && x.MarketName.StartsWith("BTC-")).ToList();

            // Remove existing trades from the list to check.
            foreach (var trade in trades)
                markets.RemoveAll(x => x.MarketName == trade.Market);

            // Remove items that are on our blacklist.
            foreach (var market in Constants.MarketBlackList)
                markets.RemoveAll(x => x.MarketName == market);

            // Check the buy signal!
            string pair = null;

            // Prioritize markets with high volume.
            foreach (var market in markets.Distinct().OrderByDescending(x => x.BaseVolume).ToList())
            {
                if (await GetBuySignal(market.MarketName))
                {
                    // A match was made, buy that please!
                    pair = market.MarketName;
                    break;
                }
            }

            // No pairs found. Return.
            if (pair == null) return null;

            var openRate = GetTargetBid(await _api.GetTicker(pair));
            var amount = amountOfBtcToInvestPerTrader / openRate;
            var amountYouGet = (amountOfBtcToInvestPerTrader * (1 - Constants.TransactionFeePercentage)) / openRate;
            var orderId = await _api.Buy(pair, openRate, amount);

            return new Trade()
            {
                Market = pair,
                StakeAmount = Constants.AmountOfBtcToInvestPerTrader,
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
        ///  Sells the current pair if the threshold is reached and updates the trade record.
        /// </summary>
        /// <param name="trade"></param>
        private async Task HandleTrade(Trade trade)
        {
            if (!trade.IsOpen)
            {
                // Trying to handle a closed trade...
                throw new Exception($"Trying to handle a closed trade {trade.Market} | PK: {trade.PartitionKey}");
            }

            var currentRate = await _api.GetTicker(trade.Market);
            var advice = await GetAdvice(trade.Market);

            if (advice != null && (advice.TradeAdvice == TradeAdvice.Sell || ShouldSell(trade, currentRate.Bid, DateTime.UtcNow) != SellType.None))
            {
                await ExecuteSell(trade, currentRate.Bid);
            }
        }

        /// <summary>
        /// Retrieves an advice (e.g. buy, sell, hold) for the given market.
        /// </summary>
        /// <param name="tradeMarket"></param>
        /// <returns></returns>
        private async Task<ITradeAdvice> GetAdvice(string tradeMarket)
        {
            var minimumDate = DateTime.UtcNow.AddHours(-120);
            var candles = await _api.GetTickerHistory(tradeMarket, minimumDate, Core.Models.Period.Hour);

            var signalDate = candles[candles.Count - 1].Timestamp;

            // This is an outdated candle...
            if (signalDate < DateTime.UtcNow.AddMinutes(-120))
                return null;

            // This calculates an advice for the next timestamp.
            var advice = _strategy.Forecast(candles.Where(x => x.Timestamp > minimumDate).ToList());

            return advice;
        }

        /// <summary>
        /// Executes a sell for the given trade and current rate.
        /// </summary>
        private async Task ExecuteSell(Trade trade, double currentRateBid)
        {
            // Get available balance
            var currency = trade.Market.Split('-')[1];
            var balance = await _api.GetBalance(currency);
            await ExecuteSellOrder(trade, currentRateBid, balance);
        }

        /// <summary>
        /// Executes a sell for the given trade and updated the entity.
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="currentRateBid">Rate to sell for</param>
        /// <param name="balance">Amount to sell for</param>
        /// <returns>Current profit as percentage</returns>
        private async Task<double> ExecuteSellOrder(Trade trade, double currentRateBid, double balance)
        {
            // Calculate our profit.
            var investment = (trade.StakeAmount * (1 - Constants.TransactionFeePercentage));
            var sales = (trade.Quantity * currentRateBid) - (trade.Quantity * currentRateBid * Constants.TransactionFeePercentage);
            var profit = 100 * ((sales - investment) / investment);

            // Sell the thing.
            var orderId = await _api.Sell(trade.Market, balance, currentRateBid);

            trade.CloseRate = currentRateBid;
            trade.CloseProfitPercentage = profit;
            trade.CloseProfit = sales - investment;
            trade.CloseDate = DateTime.UtcNow;
            trade.OpenOrderId = orderId;
            trade.SellOrderId = orderId;

            return profit;
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

            if (currentProfit < trade.StopLossAnchor)
                return SellType.StopLossAnchor;

            // Set a stop loss anchor to minimize losses.
            foreach (var item in Constants.StopLossAnchors)
            {
                if (currentProfit > item)
                    trade.StopLossAnchor = item - 0.01;
            }

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
        /// Checks if the trade is closable, and if so it is being closed.
        /// </summary>
        /// <param name="trade"></param>
        /// <returns></returns>
        private async Task<bool> CloseTradeIfFulfilled(Trade trade)
        {
            // If we don't have an open order and the close rate is already set,
            // we can close this trade.
            if (trade.CloseProfit != null && trade.CloseProfitPercentage != null && trade.CloseDate != null &&
                trade.CloseRate != null && trade.OpenOrderId == null)
            {
                // Set our balances straight.
                _dayBalance.Profit += trade.CloseProfit.Value;
                _totalBalance.Profit += trade.CloseProfit.Value;
                _totalBalance.TotalBalance += trade.CloseProfit.Value;

                _dayBalance.LastUpdated = DateTime.UtcNow;
                _totalBalance.LastUpdated = DateTime.UtcNow;

                trade.IsOpen = false;

                await SendNotification($"Sold {trade.Market} at {trade.CloseRate:0.0000000 BTC} with a profit of {trade.CloseProfitPercentage:0.00}%");

                return true;
            }

            return false;
        }

        private async Task SendNotification(string message)
        {
            if (_notification != null)
            {
                await _notification.SendNotification(message);
            }
        }
    }
}
