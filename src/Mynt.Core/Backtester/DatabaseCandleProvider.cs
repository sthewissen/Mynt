using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using Mynt.Core.Models;

namespace Mynt.Core.Backtester
{
    public class DatabaseCandleProvider
    {
        public List<Candle> GetCandles(string symbol, BacktestOptions backtestOptions)
        {
            //var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = BacktesterDatabase.GetDataDirectory(backtestOptions.DataFolder, backtestOptions.Exchange.ToString().ToLower(), symbol);

            DateTime startDate = Convert.ToDateTime(backtestOptions.StartDate);
            DateTime endDate = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(backtestOptions.EndDate))
            {
                endDate = Convert.ToDateTime(backtestOptions.EndDate);
            }

            LiteCollection<Candle> candleCollection = BacktesterDatabase.DataStore.GetInstance(filePath).GetTable<Candle>("Candle_" + backtestOptions.CandlePeriod.ToString());

            candleCollection.EnsureIndex("Timestamp");
            List<Candle> candles = candleCollection.Find(Query.Between("Timestamp", startDate, endDate), Query.Ascending).ToList();

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The .db '{filePath}' file used to load the candles from was not found.");

            return candles;
        }
    }
}