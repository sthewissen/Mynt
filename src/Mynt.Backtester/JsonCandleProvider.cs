using System;
using System.Collections.Generic;
using System.IO;
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

        public List<Candle> GetCandles(string symbol)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath, $"{folder}/{symbol}.json");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The .json '{filePath}' file used to load the candles from was not found.");

            var dataString = File.ReadAllText(filePath);
            var candles = JsonConvert.DeserializeObject<List<Candle>>(dataString);
            return candles;
        }
    }
}