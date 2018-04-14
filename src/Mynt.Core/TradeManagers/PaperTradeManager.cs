﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;
using Serilog.Core;

namespace Mynt.Core.TradeManagers
{
    public class PaperTradeManager : ITradeManager
    {
        private readonly IExchangeApi _api;
        private readonly INotificationManager _notification;
        private readonly ITradingStrategy _strategy;
        private readonly ILogger<PaperTradeManager> _logger;
        private List<Trade> _activeTrades;
        private List<Trader> _currentTraders;
        private readonly IDataStore _dataStore;
        private readonly TradeOptions _settings;


        public PaperTradeManager(IExchangeApi api, ITradingStrategy strategy, ILogger<PaperTradeManager> logger, INotificationManager notificationManager, TradeOptions settings, IDataStore dataStore)
        {
            _api = api;
            _strategy = strategy;
            _logger = logger;
            _notification = notificationManager;
            _dataStore = dataStore;
            _settings = settings;
        }

        #region SETUP

        private async Task Initialize()
        {
            // First initialize a few things                
                await _dataStore.InitializeAsync();

            _activeTrades = await _dataStore.GetActiveTradesAsync();
            _currentTraders = await _dataStore.GetBusyTradersAsync();

            // Create our trader records if no traders exist yet.
            if (_currentTraders.Count == 0)
            {
                await CreateTradersIfNoneExist();
            }

            // Get a list of our busy traders
            _currentTraders = await _dataStore.GetBusyTradersAsync();
        }

        private async Task CreateTradersIfNoneExist()
        {
            var traders = new List<Trader>();

            for (var i = 0; i < _settings.MaxNumberOfConcurrentTrades; i++)
            {
                var newTrader = new Trader()
                {
                    Identifier = $"Trader{i}",
                    CurrentBalance = _settings.AmountOfBtcToInvestPerTrader,
                    IsBusy = false,
                    LastUpdated = DateTime.UtcNow,
                    StakeAmount = _settings.AmountOfBtcToInvestPerTrader,
                };

                traders.Add(newTrader);
            }

            if (traders.Count > 0)
            {
                await _dataStore.SaveTradersAsync(traders);
            }
        }

        #endregion

        #region BUY SIDE

        /// <summary>
        /// Checks if new trades can be started.
        /// </summary>
        /// <returns></returns>
        public async Task LookForNewTrades()
        {
            // Initialize the things we'll be using throughout the process.
            await Initialize();

            // This means an order to buy has been open for an entire buy cycle.
            if (_settings.CancelUnboughtOrdersEachCycle)
                await CancelUnboughtOrders();

            // Check active trades against our strategy.
            // If the strategy tells you to sell, we create a sell.
            await CheckActiveTradesAgainstStrategy();

            // Check if there is room for more trades
            var freeTraders = await _dataStore.GetAvailableTradersAsync();

            // We have available traders to work for us!
            if (freeTraders.Count > 0)
            {
                // There's room for more.
                var trades = await FindBuyOpportunities();

                if (trades.Count > 0)
                {
                    // Depending on what we have more of we create trades.
                    var loopCount = freeTraders.Count >= trades.Count ? trades.Count : freeTraders.Count;

                    // Only create trades for our free traders
                    for (int i = 0; i < loopCount; i++)
                    {
                        await CreateNewTrade(freeTraders[i], trades[i]);
                    }
                }
                else
                {
                    _logger.LogInformation("No trade opportunities found...");
                }
            }
        }

        /// <summary>
        /// Cancels any orders that have been buying for an entire cycle.
        /// </summary>
        /// <returns></returns>
        private async Task CancelUnboughtOrders()
        {
            // Only trigger if there are orders still buying.
            if (_activeTrades.Any(x => x.IsBuying))
            {
                // Loop our current trades that are still looking to buy if there are any.
                foreach (var trade in _activeTrades.Where(x => x.IsBuying))
                {
                    // Update the buy order in our data storage.
                    trade.IsBuying = false;
                    trade.OpenOrderId = null;
                    trade.IsOpen = false;
                    trade.SellType = SellType.Cancelled;
                    trade.CloseDate = DateTime.UtcNow;

                    // Update the order
                    await _dataStore.SaveTradeAsync(trade);

                    // Handle the trader that was dedicated to this order.
                    var currentTrader = _currentTraders.FirstOrDefault(x => x.Identifier == trade.TraderId);

                    // If there is a trader, update that as well...
                    if (currentTrader != null)
                    {
                        currentTrader.IsBusy = false;
                        currentTrader.LastUpdated = DateTime.UtcNow;

                        // Update the trader to indicate that we're not busy anymore.
                        await _dataStore.SaveTraderAsync(currentTrader);
                    }

                    await SendNotification($"Cancelled {trade.Market} buy order.");
                }
            }
        }

        /// <summary>
        /// Checks our current running trades against the strategy.
        /// If the strategy tells us to sell we need to do so.
        /// </summary>
        /// <returns></returns>
        private async Task CheckActiveTradesAgainstStrategy()
        {
            // Check our active trades for a sell signal from the strategy
            foreach (var trade in _activeTrades.Where(x => (x.OpenOrderId == null || x.SellType == SellType.Immediate) && x.IsOpen))
            {
                var signal = await GetStrategySignal(trade.Market);

                // If the strategy is telling us to sell we need to do so.
                if (signal != null && signal.TradeAdvice == TradeAdvice.Sell)
                {
                    // Create a sell order for our strategy.
                    var ticker = await _api.GetTicker(trade.Market);
                    var orderId = Guid.NewGuid().ToString().Replace("-", "");

                    trade.CloseRate = ticker.Bid;
                    trade.OpenOrderId = orderId;
                    trade.SellOrderId = orderId;
                    trade.SellType = SellType.Strategy;
                    trade.IsSelling = true;

                    await _dataStore.SaveTradeAsync(trade);

                    await SendNotification($"Sell order placed for {trade.Market} at {trade.CloseRate:0.00000000} (Strategy sell).");
                }
            }
        }

        /// <summary>
        /// Checks the implemented trading indicator(s),
        /// if one pair triggers the buy signal a new trade record gets created.
        /// </summary>
        /// <returns></returns>
        private async Task<List<TradeSignal>> FindBuyOpportunities()
        {
            // Retrieve our current markets
            var markets = await _api.GetMarketSummaries();
            var pairs = new List<TradeSignal>();

            // Check if there are markets matching our volume.
            markets = markets.Where(x =>
                (x.Volume > _settings.MinimumAmountOfVolume ||
                 _settings.AlwaysTradeList.Contains(x.CurrencyPair.BaseCurrency)) &&
                 _settings.QuoteCurrencies.Contains(x.CurrencyPair.QuoteCurrency.ToUpper())).ToList();

            // If there are items on the only trade list remove the rest
            foreach (var item in _settings.OnlyTradeList)
                markets.RemoveAll(x => x.CurrencyPair.BaseCurrency != item);

            // Remove existing trades from the list to check.
            foreach (var trade in _activeTrades)
                markets.RemoveAll(x => x.MarketName == trade.Market);

            // Remove items that are on our blacklist.
            foreach (var market in _settings.MarketBlackList)
                markets.RemoveAll(x => x.CurrencyPair.BaseCurrency == market);

            // Prioritize markets with high volume.
            foreach (var market in markets.Distinct().OrderByDescending(x => x.Volume).ToList())
            {
                var signal = await GetStrategySignal(market.MarketName);

                // A match was made, buy that please!
                if (signal != null && signal.TradeAdvice == TradeAdvice.Buy)
                {
                    pairs.Add(new TradeSignal
                    {
                        MarketName = market.MarketName,
                        QuoteCurrency = market.CurrencyPair.QuoteCurrency,
                        BaseCurrency = market.CurrencyPair.BaseCurrency,
                        TradeAdvice = signal.TradeAdvice,
                        SignalCandle = signal.SignalCandle
                    });
                }
            }

            return pairs;
        }

        /// <summary>
        /// Calculates a buy signal based on several technical analysis indicators.
        /// </summary>
        /// <param name="market">The market we're going to check against.</param>
        /// <returns></returns>
        private async Task<TradeSignal> GetStrategySignal(string market)
        {
            try
            {
                var minimumDate = _strategy.GetMinimumDateTime();
                var candleDate = _strategy.GetCurrentCandleDateTime();
                var candles = await _api.GetTickerHistory(market, _strategy.IdealPeriod, minimumDate);

                // We eliminate all candles that aren't needed for the dataset incl. the last one (if it's the current running candle).
                candles = candles.Where(x => x.Timestamp >= minimumDate && x.Timestamp < candleDate).ToList();

                // Not enough candles to perform what we need to do.
                if (candles.Count < _strategy.MinimumAmountOfCandles)
                {
                    _logger.LogWarning("Not enough candle data for {Market}...", market);
                    return new TradeSignal
                    {
                        TradeAdvice = TradeAdvice.Hold,
                        MarketName = market
                    };
                }

                // Get the date for the last candle.
                var signalDate = candles[candles.Count - 1].Timestamp;

                // This is an outdated candle...
                if (signalDate < _strategy.GetSignalDate())
                {
                    _logger.LogInformation("Outdated candle for {Market}...", market);
                    return null;
                }

                // This calculates an advice for the next timestamp.
                var advice = _strategy.Forecast(candles);

                return new TradeSignal
                {
                    TradeAdvice = advice,
                    MarketName = market,
                    SignalCandle = _strategy.GetSignalCandle(candles)
                };
            }
            catch (Exception ex)
            {
                // Couldn't get a buy signal for this market, no problem. Let's skip it.
                _logger.LogError(ex, "Couldn't get buy signal for {Market}...", market);
                return null;
            }
        }

        /// <summary>
        /// Calculates bid target between current ask price and last price.
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        private decimal GetTargetBid(Ticker tick, Candle signalCandle)
        {
            if (_settings.BuyInPriceStrategy == BuyInPriceStrategy.AskLastBalance)
            {
                // If the ask is below the last, we can get it on the cheap.
                if (tick.Ask < tick.Last) return tick.Ask;

                return tick.Ask + _settings.AskLastBalance * (tick.Last - tick.Ask);
            }
            else if (_settings.BuyInPriceStrategy == BuyInPriceStrategy.SignalCandleClose)
            {
                return signalCandle.Close;
            }
            else if (_settings.BuyInPriceStrategy == BuyInPriceStrategy.MatchCurrentBid)
            {
                return tick.Bid;
            }
            else
            {
                return Math.Round(tick.Bid * (1 - _settings.BuyInPricePercentage), 8);
            }
        }

        /// <summary>
        /// Creates a new trade in our system and opens a buy order.
        /// </summary>
        /// <returns></returns>
        private async Task CreateNewTrade(Trader freeTrader, TradeSignal signal)
        {
            // We don't get a balance from the exchange, let's just assume it is always enough.
            var currentQuoteBalance = 9999;

            // Do we even have enough funds to invest?
            if (currentQuoteBalance < freeTrader.CurrentBalance)
            {
                _logger.LogWarning("Insufficient funds ({Available}) to perform a {MarketName} trade. Skipping this trade.", currentQuoteBalance, signal.MarketName);
                return;
            }

            var order = await CreateBuyOrder(freeTrader, signal.MarketName, signal.SignalCandle);

            // We found a trade and have set it all up!
            if (order != null)
            {
                // Save the order.
                await _dataStore.SaveTradeAsync(order);

                // Send a notification that we found something suitable
                _logger.LogInformation("New trade signal {market}...", order.Market);

                // Update the trader to busy
                freeTrader.LastUpdated = DateTime.UtcNow;
                freeTrader.IsBusy = true;

                // Save the new trader state.
                await _dataStore.SaveTraderAsync(freeTrader);
            }
        }

        /// <summary>
        /// Creates a buy order on the exchange.
        /// </summary>
        /// <param name="freeTrader">The trader placing the order</param>
        /// <param name="pair">The pair we're buying</param>
        /// <returns></returns>
        private async Task<Trade> CreateBuyOrder(Trader freeTrader, string pair, Candle signalCandle)
        {
            // Take the amount to invest per trader OR the current balance for this trader.
            var btcToSpend = freeTrader.CurrentBalance > _settings.AmountOfBtcToInvestPerTrader
                ? _settings.AmountOfBtcToInvestPerTrader
                : freeTrader.CurrentBalance;

            // The amount here is an indication and will probably not be precisely what you get.
            var ticker = await _api.GetTicker(pair);
            var openRate = GetTargetBid(ticker, signalCandle);
            var amount = btcToSpend / openRate;

            // Get the order ID, this is the most important because we need this to check
            // up on our trade. We update the data below later when the final data is present.
            var orderId = GetOrderId();

            await SendNotification($"Buying {pair} at ±{openRate:0.00000000} which was spotted at bid: {ticker.Bid:0.00000000}, " +
                               $"ask: {ticker.Ask:0.00000000}, " +
                               $"last: {ticker.Last:0.00000000}, " +
                               $"({amount:0.0000} units).");

            var trade = new Trade()
            {
                TraderId = freeTrader.Identifier,
                Market = pair,
                StakeAmount = btcToSpend,
                OpenRate = openRate,
                OpenDate = DateTime.UtcNow,
                Quantity = amount,
                OpenOrderId = orderId,
                BuyOrderId = orderId,
                IsOpen = true,
                IsBuying = true,
                StrategyUsed = _strategy.Name,
                SellType = SellType.None,
            };

            if (_settings.PlaceFirstStopAtSignalCandleLow)
            {
                trade.StopLossRate = signalCandle.Low;
                _logger.LogInformation("Automatic stop set at signal candle low {Low}", signalCandle.Low.ToString("0.00000000"));
            }

            return trade;
        }

        #endregion

        #region SELL SIDE
        
        public async Task UpdateExistingTrades()
        {
            // Get our current trades.
          
            await Initialize();

            // First we update our open buy orders by checking if they're filled.
            await UpdateOpenBuyOrders();

            // Secondly we check if currently selling trades can be marked as sold if they're filled.
            await UpdateOpenSellOrders();

            // Third, our current trades need to be checked if one of these has hit its sell targets...
            await CheckForSellConditions();
        }

        /// <summary>
        /// Updates the buy orders by checking with the exchange what status they are currently.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateOpenBuyOrders()
        {
            // There are trades that have an open order ID set & no sell order id set
            // that means its a buy trade that is waiting to get bought. See if we can update that first.
            foreach (var trade in _activeTrades.Where(x => x.OpenOrderId != null && x.SellOrderId == null))
            {
                var candles = await _api.GetTickerHistory(trade.Market, Period.Minute, trade.OpenDate);
                var candlesContainingBuy = candles.Where(x => trade.OpenRate >= x.High ||(trade.OpenRate >= x.Low && trade.OpenRate <= x.High)).ToList();

                _logger.LogInformation($"Checking {trade.Market} BUY order @ {trade.OpenRate:0.00000000}...");

                // This means the order probably would've gotten filled...
                // We have no other way to check this, because no actual orders are being placed.
                if (candlesContainingBuy.Any())
                {
                    trade.OpenOrderId = null;
                    trade.IsBuying = false;
                    
                    _logger.LogInformation($"{trade.Market} BUY order filled @ {trade.OpenRate:0.00000000}...");

                    // If this is enabled we place a sell order as soon as our buy order got filled.
                    if (_settings.ImmediatelyPlaceSellOrder)
                    {
                        var sellPrice = Math.Round(trade.OpenRate * (1 + _settings.ImmediatelyPlaceSellOrderAtProfit), 8);
                        var orderId = GetOrderId();

                        trade.CloseRate = sellPrice;
                        trade.OpenOrderId = orderId;
                        trade.SellOrderId = orderId;
                        trade.IsSelling = true;
                        trade.SellType = SellType.Immediate;
                        
                        _logger.LogInformation($"{trade.Market} order placed @ {trade.CloseRate:0.00000000}...");
                    }

                    await _dataStore.SaveTradeAsync(trade);

                    await SendNotification($"Buy order filled for {trade.Market} at {trade.OpenRate:0.00000000}.");
                }
            }
        }

        /// <summary>
        /// Updates the sell orders by checking with the exchange what status they are currently.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateOpenSellOrders()
        {
            // There are trades that have an open order ID set & sell order id set
            // that means its a sell trade that is waiting to get sold. See if we can update that first.

            foreach (var order in _activeTrades.Where(x => x.OpenOrderId != null && x.SellOrderId != null))
            {
                var candles = await _api.GetTickerHistory(order.Market, Period.Minute, order.OpenDate);
                var candlesContainingSell = candles.Where(x =>order.CloseRate <= x.Low || (order.CloseRate >= x.Low && order.CloseRate <= x.High)).ToList();

                _logger.LogInformation($"Checking {order.Market} SELL order @ {order.CloseRate:0.00000000}...");

                // This means the order probably would've gotten filled...
                // We have no other way to check this, because no actual orders are being placed.
                if (candlesContainingSell.Any())
                {
                    order.OpenOrderId = null;
                    order.IsOpen = false;
                    order.IsSelling = false;
                    order.CloseDate = DateTime.UtcNow;

                    _logger.LogInformation($"{order.Market} SELL order filled @ {order.CloseRate:0.00000000}...");

                    order.CloseProfit = (order.CloseRate * order.Quantity) - order.StakeAmount;
                    order.CloseProfitPercentage = ((order.CloseRate * order.Quantity) - order.StakeAmount) / order.StakeAmount * 100;

                    // Retrieve the trader responsible for this trade
                    var trader = _currentTraders.FirstOrDefault(x => x.Identifier == order.TraderId);

                    if (trader != null)
                    {
                        trader.IsBusy = false;
                        trader.CurrentBalance += order.CloseProfit.Value;
                        trader.CurrentBalance = Math.Round(trader.CurrentBalance, 8);
                        trader.LastUpdated = DateTime.UtcNow;
                    }

                    await _dataStore.SaveTraderAsync(trader);
                    await _dataStore.SaveTradeAsync(order);

                    await SendNotification($"Sold {order.Market} at {order.CloseRate:0.00000000} for {order.CloseProfit:0.00000000} profit ({order.CloseProfitPercentage:0.00}%).");
                }
            }
        }
                                    
        /// <summary>
        /// Checks the current active trades if they need to be sold.
        /// </summary>
        /// <returns></returns>
        private async Task CheckForSellConditions()
        {
            // There are trades that have no open order ID set & are still open.
            // that means its a trade that is waiting to get sold. See if we can update that first.

            // An open order currently not selling or being an immediate sell are checked for SL  etc.
            foreach (var trade in _activeTrades.Where(x => (x.OpenOrderId == null || x.SellType == SellType.Immediate) && x.IsOpen))
            {
                // These are trades that are not being bought or sold at the moment so these need to be checked for sell conditions.
                var ticker = await _api.GetTicker(trade.Market);
                var sellType = ShouldSell(trade, ticker.Bid, DateTime.UtcNow);
                
                _logger.LogInformation($"Checking {trade.Market} sell conditions...");

                if (sellType != SellType.None)
                {
                    var orderId = GetOrderId();

                    trade.CloseRate = ticker.Bid;
                    trade.OpenOrderId = orderId;
                    trade.SellOrderId = orderId;
                    trade.SellType = sellType;
                    trade.IsSelling = true;
                    
                    _logger.LogInformation($"Selling {trade.Market} ({sellType.ToString()})...");

                    await _dataStore.SaveTradeAsync(trade);

                    await SendNotification($"Going to sell {trade.Market} at {trade.CloseRate:0.00000000}.");
                }
                else if (sellType == SellType.TrailingStopLossUpdated)
                {
                    // Update the stop loss for this trade, which was set in ShouldSell.
                    _logger.LogInformation("Updated the trailing stop loss for {Market} to {StopLoss}", trade.Market, trade.StopLossRate.Value.ToString("0.00000000"));
                    await _dataStore.SaveTradeAsync(trade);
                }
            }
        }

        /// <summary>
        /// Based on earlier trade and current price and configuration, decides whether bot should sell.
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="currentRateBid"></param>
        /// <param name="utcNow"></param>
        /// <returns>True if bot should sell at current rate.</returns>
        private SellType ShouldSell(Trade trade, decimal currentRateBid, DateTime utcNow)
        {
            var currentProfit = (currentRateBid - trade.OpenRate) / trade.OpenRate;

            _logger.LogInformation("Should sell {Market}? Profit: {Profit}%...", trade.Market, (currentProfit * 100).ToString("0.00"));

            // Let's not do a stoploss for now...
            if (currentProfit < _settings.StopLossPercentage)
            {
                _logger.LogInformation("Stop loss hit: {StopLoss}%", _settings.StopLossPercentage);
                return SellType.StopLoss;
            }

            // Check if time matches and current rate is above threshold
            foreach (var item in _settings.ReturnOnInvestment)
            {
                var timeDiff = (utcNow - trade.OpenDate).TotalSeconds / 60;

                if (timeDiff > item.Duration && currentProfit > item.Profit)
                {
                    _logger.LogInformation("Timer hit: {TimeDifference} mins, profit {Profit}%", timeDiff, item.Profit.ToString("0.00"));
                    return SellType.Timed;
                }
            }

            // Only run this when we're past our starting percentage for trailing stop.
            if (_settings.EnableTrailingStop)
            {
                // If the current rate is below our current stoploss percentage, close the trade.
                if (trade.StopLossRate.HasValue && currentRateBid < trade.StopLossRate.Value)
                    return SellType.TrailingStopLoss;

                // The new stop would be at a specific percentage above our starting point.
                var newStopRate = trade.OpenRate * (1 + (currentProfit - _settings.TrailingStopPercentage));

                // Only update the trailing stop when its above our starting percentage and higher than the previous one.
                if (currentProfit > _settings.TrailingStopStartingPercentage && (trade.StopLossRate < newStopRate || !trade.StopLossRate.HasValue))
                {
                    // The current profit percentage is high enough to create the trailing stop value.
                    trade.StopLossRate = newStopRate;

                    return SellType.TrailingStopLossUpdated;
                }

                return SellType.None;
            }

            return SellType.None;
        }

        private static string GetOrderId()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
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
