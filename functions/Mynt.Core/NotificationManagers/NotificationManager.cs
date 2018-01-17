using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mynt.Core.Api;
using Mynt.Core.Extensions;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.NotificationManagers
{
    public class NotificationManager
    {
        private readonly IExchangeApi _api;
        private readonly ITradingStrategy _strategy;
        private readonly Action<string> _log;

        public NotificationManager(IExchangeApi api, ITradingStrategy strategy, Action<string> log)
        {
            _api = api;
            _strategy = strategy;
            _log = log;
        }
        
        /// <summary>
        /// List everything that gives a positive trade signal.
        /// </summary>
        private async Task<List<string>> ListTrades()
        {
            var results = new List<string>();

            // Retrieve our current markets
            var markets = await _api.GetMarketSummaries();

            // Check if there are markets matching our volume.
            markets = markets.Where(x => (x.BaseVolume > Constants.MinimumAmountOfVolume ||
                         Constants.AlwaysTradeList.Contains(x.MarketName)) && x.MarketName.StartsWith("BTC-")).ToList();

            // Remove items that are on our blacklist.
            foreach (var market in Constants.MarketBlackList)
                markets.RemoveAll(x => x.MarketName == market);
            
            // Prioritize markets with high volume.
            foreach (var market in markets.Distinct().OrderByDescending(x => x.BaseVolume).ToList())
            {
                try
                {
                    var trend = await GetTrend(market.MarketName);
                    if (trend.Count > 0 && trend.Last() == 1)
                        // A match was made, buy that please!
                        results.Add(market.MarketName);
                }
                catch {
                    // Couldn't get a trend, no worries, move on.
                }
            }

            return results;
        }

        /// <summary>
        /// Retrieves a trend list for the given market.
        /// </summary>
        /// <param name="tradeMarket"></param>
        /// <returns></returns>
        private async Task<List<int>> GetTrend(string tradeMarket)
        {
            var minimumDate = DateTime.UtcNow.AddHours(-120);
            var candles = await _api.GetTickerHistory(tradeMarket, minimumDate, Period.Hour);

            _strategy.Candles = candles.Where(x => x.Timestamp > minimumDate).ToList();

            var signalDate = candles[candles.Count - 1].Timestamp;

            // This is an outdated candle...
            if (signalDate < DateTime.UtcNow.AddMinutes(-120))
                return new List<int>() { };

            // This calculates a buy signal for each candle.
            var trend = _strategy.Prepare();

            return trend;
        }

        public async Task Process()
        {
            var buySignals = await ListTrades();

            var pushManager = new PushNotificationManager();
             var slackManager = new SlackNotificationManager();

            if (buySignals.Count == 0)
            {
                await slackManager.SendTemplatedNotification(@"No possible trends found...");
                return;
            }

            foreach (var potentialTrade in buySignals)
            {
                await slackManager.SendTemplatedNotification(@"_Experimental:_ Possible trend coming up for *{0}* - <https://bittrex.com/Market/Index?MarketName={0}>", potentialTrade);
                await pushManager.SendTemplatedNotification(@"Possible trend coming up for {0}!", potentialTrade);
             }
        }
    }
}
