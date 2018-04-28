using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Models;
using Newtonsoft.Json;

namespace Mynt.Backtester
{
    internal class DataRefresher
    {
        private readonly ExchangeOptions _exchangeOptions;
        private readonly BaseExchange _api;

        public DataRefresher(ExchangeOptions exchangeOptions)
        {
            _exchangeOptions = exchangeOptions;
            _api = new BaseExchange(_exchangeOptions);
        }

        private string GetDataDirectory()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(basePath, "data");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        public bool CheckForCandleData()
        {
            return Directory.GetFiles(GetDataDirectory(), "*.json", SearchOption.TopDirectoryOnly).Count() != 0;
        }

        private string GetJsonFilePath(string pair)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(GetDataDirectory(), $"{pair}.json");
        }

        public async Task RefreshCandleData(List<string> coinsToRefresh, Action<string> callback)
        {
             List<string> writtenFiles = new List<string>();

            foreach (var coinToBuy in coinsToRefresh)
            { 
                var startDate = DateTime.UtcNow.AddHours(-500);
                var jsonPath = GetJsonFilePath(coinToBuy);

                // Delete an existing file.
                if (File.Exists(jsonPath)) File.Delete(jsonPath);

                var i = 2;
                DateTime endDate;
                var totalCandles = new List<Candle>();
                var candles = await _api.GetTickerHistory(coinToBuy, Period.Hour, startDate);
                totalCandles.AddRange(candles);

                // Get these in batches of 500 because they're limited in the API.
                while (candles.Count != 0)
                {
                    endDate = startDate;
                    startDate = DateTime.UtcNow.AddHours(-500 * i);
                    candles = await _api.GetTickerHistory(coinToBuy, Period.Hour, startDate, endDate);
                    i += 1;
                    totalCandles.AddRange(candles);
                }

                // Add the last bit in...
                totalCandles.AddRange(candles);
                totalCandles = totalCandles.OrderBy(x => x.Timestamp).ToList();

                // Write all the text.
                File.WriteAllText(jsonPath, JsonConvert.SerializeObject(totalCandles));
                writtenFiles.Add(jsonPath);

                callback($"Refreshed data for {coinToBuy}...");
            }

            // Delete everything that's not refreshed
            foreach (FileInfo fi in new DirectoryInfo(GetDataDirectory()).EnumerateFiles())
            {
                if (!writtenFiles.Contains(fi.FullName))
                    File.Delete(fi.FullName);
            }
        }

        private TimeSpan GetCacheAge()
        {
            string dataFolder = Path.GetDirectoryName(GetJsonFilePath("dummy-dummy"));

            if (Directory.GetFiles(dataFolder).Length == 0)
                return TimeSpan.MinValue;

            var fileInfo = new DirectoryInfo(dataFolder).GetFileSystemInfos()
                                                        .OrderBy(fi => fi.CreationTime)
                                                        .First();

            return DateTime.Now - fileInfo.LastWriteTime;
        }
    }
}
