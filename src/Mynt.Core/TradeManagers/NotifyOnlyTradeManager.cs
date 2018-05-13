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
        private readonly List<ITradingStrategy> _strategy;
        private readonly ILogger _logger;
        private readonly TradeOptions _settings;

        public NotifyOnlyTradeManager(IExchangeApi api, INotificationManager notificationManager, ILogger logger, TradeOptions settings, params ITradingStrategy[] strategies)
        {
            _api = api;
            _strategy = strategies.ToList();
            _logger = logger;
            _notification = notificationManager;
            _settings = settings;
        }

        /// <summary>
        /// Checks if new trades can be started.
        /// </summary>
        /// <returns></returns>
        public async Task LookForNewTrades()
        {
            await FindBuyOpportunities();
        }

        private async Task CreateTradeSignal(TradeSignal trade)
        {
            // The amount here is an indication and will probably not be precisely what you get.
            var ticker = await _api.GetTicker(trade.MarketName);
            var openRate = GetTargetBid(ticker, trade.SignalCandle);
            var stop = openRate * (1 + _settings.StopLossPercentage);

            if (trade.TradeAdvice == TradeAdvice.Buy)
            {
                if (trade.Strategy is INotificationTradingStrategy)
                {
                    await SendNotification($"✅ {trade.Strategy.Name} - #{trade.MarketName} at {openRate:0.00000000}\n{((INotificationTradingStrategy)trade.Strategy).BuyMessage}");
                }
                else
                {
                    await SendNotification($"🆘 {trade.Strategy.Name} - #{trade.MarketName} at {openRate:0.00000000}");
                }
            }
            else if (trade.TradeAdvice == TradeAdvice.Sell)
            {
                if (trade.Strategy is INotificationTradingStrategy)
                {
                    await SendNotification($"🆘 {trade.Strategy.Name} - #{trade.MarketName} at {openRate:0.00000000}\n{((INotificationTradingStrategy)trade.Strategy).SellMessage}");
                }
                else
                {
                    await SendNotification($"🆘 {trade.Strategy.Name} - #{trade.MarketName} at {openRate:0.00000000}");
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
                 _settings.QuoteCurrency.Contains(x.CurrencyPair.QuoteCurrency.ToUpper())).ToList();

            // Remove items that are on our blacklist.
            foreach (var market in _settings.MarketBlackList)
                markets.RemoveAll(x => x.CurrencyPair.BaseCurrency == market);

            // If there are items on the only trade list remove the rest
            foreach (var item in _settings.OnlyTradeList)
                markets.RemoveAll(x => x.CurrencyPair.BaseCurrency != item);

            // Prioritize markets with high volume.
            foreach (var market in markets.Distinct().OrderByDescending(x => x.Volume).ToList())
            {
                var signals = await GetStrategySignals(market.MarketName);

                foreach (var item in signals)
                {
                    if (item.TradeAdvice != TradeAdvice.Hold)
                    {
                        await CreateTradeSignal(item);
                    }
                }
            }

            return pairs;
        }

        /// <summary>
        /// Calculates a buy signal based on several technical analysis indicators.
        /// </summary>
        /// <param name="market">The market we're going to check against.</param>
        /// <returns></returns>
        private async Task<List<TradeSignal>> GetStrategySignals(string market)
        {
            try
            {
                _logger.LogInformation("Checking market {Market}...", market);

                var results = new List<TradeSignal>();

                foreach (var strategy in _strategy)
                {
                    var minimumDate = strategy.GetMinimumDateTime();
                    var candleDate = strategy.GetCurrentCandleDateTime();
                    var candles = await _api.GetTickerHistory(market, strategy.IdealPeriod, minimumDate);

                    // We eliminate all candles that aren't needed for the dataset incl. the last one (if it's the current running candle).
                    candles = candles.Where(x => x.Timestamp >= minimumDate && x.Timestamp < candleDate).ToList();

                    // Not enough candles to perform what we need to do.
                    if (candles.Count < strategy.MinimumAmountOfCandles)
                        continue;

                    // Get the date for the last candle.
                    var signalDate = candles[candles.Count - 1].Timestamp;

                    // This is an outdated candle...
                    if (signalDate < strategy.GetSignalDate())
                        return null;

                    // This calculates an advice for the next timestamp.
                    var advice = strategy.Forecast(candles);

                    results.Add(new TradeSignal
                    {
                        TradeAdvice = advice,
                        MarketName = market,
                        SignalCandle = strategy.GetSignalCandle(candles),
                        Strategy = strategy
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                // Couldn't get a buy signal for this market, no problem. Let's skip it.
                _logger.LogError(ex, "Couldn't get buy signal for {Market}...", market);
                return new List<TradeSignal>();
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