using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Models;
using Mynt.DataAccess.Interfaces;
using Newtonsoft.Json;

namespace Mynt.DataAccess.FileBasedStorage
{
    public class JsonCandleProvider : ICandleStorage
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
