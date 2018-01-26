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
    public class JsonCandleProvider : ICandleProvider
    {
        private string folder;

        public JsonCandleProvider(string folder)
        {
            this.folder = folder;
        }

        public List<Candle> GetCandles(string symbol)
        {
            var dataString = File.ReadAllText($"{folder}/{symbol}.json");            
            var candles = JsonConvert.DeserializeObject<List<Candle>>(dataString);
            return candles;
        }
    }
}
