using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Backtester
{
    public class DatabaseCandleProvider
    {
        public async Task<List<Candle>> GetCandles(string symbol, BacktestOptions backtestOptions, IDataStore dataStore)
        {
            DateTime startDate = Convert.ToDateTime(backtestOptions.StartDate);
            DateTime endDate = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(backtestOptions.EndDate))
            {
                endDate = Convert.ToDateTime(backtestOptions.EndDate);
            }

            List<Candle> candles = await dataStore.GetBacktestCandlesBetweenTime(backtestOptions, symbol, startDate, endDate);

            return candles;
        }
    }
}