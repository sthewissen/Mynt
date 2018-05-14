using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Extensions;
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
            return Directory.GetFiles(GetDataDirectory(), "*.db", SearchOption.TopDirectoryOnly).Count() != 0;
        }

        private string GetJsonFilePath(string pair)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(GetDataDirectory(), $"{pair}.db");
        }

        public async Task RefreshCandleData(List<string> coinsToRefresh, Action<string> callback, bool updateCandles, int period)
        {
             List<string> writtenFiles = new List<string>();

            foreach (var coinToBuy in coinsToRefresh)
            {
                DateTime startDate = Convert.ToDateTime(Program.BacktestOptions.StartDate);
                DateTime endDate = DateTime.UtcNow;
                var jsonPath = GetJsonFilePath(coinToBuy);

                LiteDatabase database = new LiteDatabase(jsonPath);
                LiteCollection<Candle> candleCollection = database.GetCollection<Candle>("Candle_" + period.ToString());

                // Delete an existing file if this is no update
                if (!updateCandles)
                {
                    if (File.Exists(jsonPath))
                    {
                        File.Delete(jsonPath);
                    } 
                    callback($"Recreate database with Period {period.ToString()}min for {coinToBuy.ToString()} from {startDate.ToString()} UTC to {endDate.RoundDown(TimeSpan.FromMinutes(period))} UTC");
                }
                else
                {
                    candleCollection.EnsureIndex("Timestamp");
                    Candle currentHistoricalData = candleCollection.Find(Query.All("Timestamp", Query.Descending), limit: 1).FirstOrDefault();
                    if (currentHistoricalData != null)
                    {
                        startDate = currentHistoricalData.Timestamp.ToUniversalTime();
                        callback($"Update database with Period {period.ToString()}min for {coinToBuy.ToString()} from {startDate.ToString()} UTC to {endDate.RoundDown(TimeSpan.FromMinutes(period))} UTC");
                    } else
                    {
                        callback($"Create new database with Period {period.ToString()}min for {coinToBuy.ToString()} from {startDate.ToString()} UTC to {endDate.RoundDown(TimeSpan.FromMinutes(period))} UTC");
                    }
                }

                // Get these in batches of 500 because they're limited in the API.
                while (startDate < endDate.RoundDown(TimeSpan.FromMinutes(period)))
                {
                    List<Candle> candles = await _api.GetTickerHistory(coinToBuy, period.FromMinutesEquivalent(), startDate, endDate);
                    startDate = candles.LastOrDefault().Timestamp;
                    candleCollection.InsertBulk(candles);
                }

                callback($"Refreshed data for {coinToBuy}...");
                writtenFiles.Add(jsonPath);
            }

            // Delete everything that's not refreshed
            foreach (FileInfo fi in new DirectoryInfo(GetDataDirectory()).EnumerateFiles())
            {
                if (!writtenFiles.Contains(fi.FullName))
                {
                    File.Delete(fi.FullName);
                }
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
