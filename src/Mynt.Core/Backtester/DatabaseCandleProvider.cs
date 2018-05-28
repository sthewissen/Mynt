using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Backtester
{
    public class DatabaseCandleProvider
    {
        public async Task<List<Candle>> GetCandles(BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            backtestOptions.EndDate = DateTime.UtcNow;

            if (backtestOptions.EndDate != DateTime.MinValue)
            {
                backtestOptions.EndDate = backtestOptions.EndDate;
            }

            List<Candle> candles = await dataStore.GetBacktestCandlesBetweenTime(backtestOptions);

            return candles;
        }
    }
}