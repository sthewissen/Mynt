using Mynt.Core.Api;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mynt.Core.TradeManagers
{
    public class NotifyOnlyTradeManager
    {
        private readonly IExchangeApi _api;
        private readonly INotificationManager _notification;
        private readonly ITradingStrategy _strategy;
        private readonly Action<string> _log;

        public NotifyOnlyTradeManager(IExchangeApi api, ITradingStrategy strat, INotificationManager notificationManager,
            Action<string> log)
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
        public async Task CheckStrategySignals()
        {
            var trades = await FindBuyOpportunities();

            if (trades.Count > 0)
            {
                foreach (var trade in trades)
                {
                    // Depending on what we have more of we create trades.
                    // The amount here is an indication and will probably not be precisely what you get.
                    var ticker = await _api.GetTicker(trade.Pair);
                    var openRate = GetTargetBid(ticker, trade.SignalCandle);
                    var stop = openRate * (1 + Constants.StopLossPercentage);

                    // Get the order ID, this is the most important because we need this to check
                    // up on our trade. We update the data below later when the final data is present.
                    await SendNotification($"{_strategy.Name}: Buy some {trade.Pair} at {openRate:0.0000000 BTC}.");
                }
            }
            else
            {
                _log("No trade opportunities found...");
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
                (x.BaseVolume > Constants.MinimumAmountOfVolume ||
                Constants.AlwaysTradeList.Contains(x.CurrencyPair.BaseCurrency)) &&
                x.CurrencyPair.QuoteCurrency.ToUpper() == "BTC").ToList();

            // Remove items that are on our blacklist.
            foreach (var market in Constants.MarketBlackList)
                markets.RemoveAll(x => x.CurrencyPair.BaseCurrency == market);

            // Prioritize markets with high volume.
            foreach (var market in markets.Distinct().OrderByDescending(x => x.BaseVolume).ToList())
            {
                var signal = await GetStrategySignal(market.MarketName);

                // A match was made, buy that please!
                if (signal != null && signal.TradeAdvice.TradeAdvice == TradeAdvice.Buy)
                {
                    pairs.Add(new TradeSignal()
                    {
                        Pair = market.MarketName,
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
                _log($"Checking market {market}...");

                var minimumDate = _strategy.GetMinimumDateTime();
                var candleDate = _strategy.GetCurrentCandleDateTime();
                var candles = await _api.GetTickerHistory(market, _strategy.IdealPeriod, minimumDate);

                // We eliminate all candles that aren't needed for the dataset incl. the last one (if it's the current running candle).
                candles = candles.Where(x => x.Timestamp >= minimumDate && x.Timestamp < candleDate).ToList();

                // Not enough candles to perform what we need to do.
                if (candles.Count < _strategy.MinimumAmountOfCandles)
                    return new TradeSignal
                    {
                        TradeAdvice = new SimpleTradeAdvice(TradeAdvice.Hold),
                        Pair = market
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
                    Pair = market,
                    SignalCandle = _strategy.GetSignalCandle(candles)
                };
            }
            catch (Exception)
            {
                // Couldn't get a buy signal for this market, no problem. Let's skip it.
                _log($"Couldn't get buy signal for {market}...");
                return null;
            }
        }

        /// <summary>
        /// Calculates bid target between current ask price and last price.
        /// </summary>
        /// <returns></returns>
        private double GetTargetBid(Ticker tick, Candle signalCandle)
        {
            if (Constants.BuyInPriceStrategy == BuyInPriceStrategy.AskLastBalance)
            {
                // If the ask is below the last, we can get it on the cheap.
                if (tick.Ask < tick.Last) return tick.Ask;

                return tick.Ask + Constants.AskLastBalance * (tick.Last - tick.Ask);
            }
            else if (Constants.BuyInPriceStrategy == BuyInPriceStrategy.SignalCandleClose)
            {
                return signalCandle.Close;
            }
            else if (Constants.BuyInPriceStrategy == BuyInPriceStrategy.MatchCurrentBid)
            {
                return tick.Bid;
            }
            else
            {
                return Math.Round(tick.Bid * (1 - Constants.BuyInPricePercentage), 8);
            }
        }

        public async Task UpdateRunningTrades()
        {
            await Task.FromResult(true);
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

