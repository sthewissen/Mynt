using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Mynt.Core.TradeManagers
{
    public class NotifyOnlyTradeManager : ITradeManager
    {
        private readonly IExchangeApi _api;
        private readonly INotificationManager _notification;
        private readonly string _buyMessage;
        private readonly string _sellMessage;
        private readonly ITradingStrategy _strategy;
        private readonly ILogger _logger;
        private readonly TradeOptions _settings;

        public NotifyOnlyTradeManager(IExchangeApi api, ITradingStrategy strategy, INotificationManager notificationManager, string buyMessage, string sellMessage, ILogger logger, TradeOptions settings)
        {
            _api = api;
            _strategy = strategy;
            _logger = logger;
            _notification = notificationManager;
            _buyMessage = buyMessage;
            _sellMessage = sellMessage;
            _settings = settings;
        }

        /// <summary>
        /// Checks if new trades can be started.
        /// </summary>
        /// <returns></returns>
        public async Task LookForNewTrades()
        {
            var trades = await FindBuyOpportunities();

            if (trades.Count > 0)
            {
                foreach (var trade in trades)
                {
                    // Depending on what we have more of we create trades.
                    // The amount here is an indication and will probably not be precisely what you get.
                    var ticker = await _api.GetTicker(trade.MarketName);
                    var openRate = GetTargetBid(ticker, trade.SignalCandle);
                    var stop = openRate * (1 + _settings.StopLossPercentage);

                    if (trade.TradeAdvice == TradeAdvice.Buy)
                    {
                        await SendNotification($"ℹ️ {_strategy.Name} - #{trade.MarketName} at {openRate:0.00000000}\n" + _buyMessage);
                    }
                    else if (trade.TradeAdvice == TradeAdvice.Sell)
                    {
                        await SendNotification($"ℹ️ {_strategy.Name} - #{trade.MarketName} at {openRate:0.00000000}\n" + _sellMessage);
                    }
                }
            }
            else
            {
                _logger.LogInformation("No trade opportunities found...");
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
                 _settings.QuoteCurrency.Contains(x.CurrencyPair.QuoteCurrency.ToUpper())).ToList();

            // Remove items that are on our blacklist.
            foreach (var market in _settings.MarketBlackList)
                markets.RemoveAll(x => x.CurrencyPair.BaseCurrency == market);

            // Prioritize markets with high volume.
            foreach (var market in markets.Distinct().OrderByDescending(x => x.Volume).ToList())
            {
                var signal = await GetStrategySignal(market.MarketName);

                if (signal != null && signal.TradeAdvice != TradeAdvice.Hold)
                {
                    pairs.Add(new TradeSignal()
                    {
                        MarketName = market.MarketName,
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
                _logger.LogInformation("Checking market {Market}...", market);

                var minimumDate = _strategy.GetMinimumDateTime();
                var candleDate = _strategy.GetCurrentCandleDateTime();
                var candles = await _api.GetTickerHistory(market, _strategy.IdealPeriod, minimumDate);

                // We eliminate all candles that aren't needed for the dataset incl. the last one (if it's the current running candle).
                candles = candles.Where(x => x.Timestamp >= minimumDate && x.Timestamp < candleDate).ToList();

                // Not enough candles to perform what we need to do.
                if (candles.Count < _strategy.MinimumAmountOfCandles)
                    return new TradeSignal
                    {
                        TradeAdvice = TradeAdvice.Hold,
                    MarketName = market
                    };

                // Get the date for the last candle.
                var signalDate = candles[candles.Count - 1].Timestamp;

                // This is an outdated candle...
                if (signalDate < _strategy.GetSignalDate())
                    return null;

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

        private async Task SendNotification(string message)
        {
            if (_notification != null)
            {
                await _notification.SendNotification(message);
            }
        }

        public Task UpdateExistingTrades()
        {
            return Task.CompletedTask;
        }
    }
}