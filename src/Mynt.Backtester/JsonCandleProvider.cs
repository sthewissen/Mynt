using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Models;
using Newtonsoft.Json;

namespace Mynt.Backtester
{
    internal class JsonCandleProvider
    {
        private readonly string folder;

        public JsonCandleProvider(string folder)
        {
            this.folder = folder;
        }

        public List<Candle> GetCandles(string symbol, int period)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath, $"{folder}/{symbol}.db");

            DateTime startDate = Convert.ToDateTime(Program.BacktestOptions.StartDate);
            DateTime endDate = DateTime.UtcNow;

            if (Program.BacktestOptions.EndDate != null && Program.BacktestOptions.EndDate != "")
            {
                endDate = Convert.ToDateTime(Program.BacktestOptions.EndDate);
            }

            LiteDatabase database = new LiteDatabase(filePath);
            LiteCollection<Candle> candleCollection = database.GetCollection<Candle>("Candle_" + period);
            candleCollection.EnsureIndex("Timestamp");
            List<Candle> candles = candleCollection.Find(Query.Between("Timestamp", startDate, endDate), Query.Ascending).ToList();

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The .db '{filePath}' file used to load the candles from was not found.");

            return candles;
        }
    }
}