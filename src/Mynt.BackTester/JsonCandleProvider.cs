using Mynt.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mynt.BackTester
{
    public class JsonCandleProvider
    {
        private string folder;

        public JsonCandleProvider(string folder)
        {
            this.folder = folder;
        }

        public List<Candle> GetCandles(string symbol)
        {
            string filePath = $"{folder}/{symbol}.json";

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The .json '{filePath}' file used to load the candles from was not found.");

            var dataString = File.ReadAllText(filePath);
            var candles = JsonConvert.DeserializeObject<List<Candle>>(dataString);
            return candles;
        }
    }
}
