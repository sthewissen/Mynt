using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Models;
using Newtonsoft.Json;

namespace Mynt.Core.Backtester
{
    public class DatabaseCandleProvider
    {
        private readonly string folder;

        public DatabaseCandleProvider(string folder)
        {
            this.folder = folder;
        }

        public List<Candle> GetCandles(string symbol, BacktestOptions backtestOptions)
        {
            //var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = BacktesterDatabase.GetDataDirectory(backtestOptions.Exchange.ToLower(), symbol);

            DateTime startDate = Convert.ToDateTime(backtestOptions.StartDate);
            DateTime endDate = DateTime.UtcNow;

            if (backtestOptions.EndDate != null && backtestOptions.EndDate != "")
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