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
    public class DataRefresher
    {
        private readonly ExchangeOptions _exchangeOptions;
        private readonly BaseExchange _api;

        public DataRefresher(ExchangeOptions exchangeOptions)
        {
            _exchangeOptions = exchangeOptions;
            _api = new BaseExchange(_exchangeOptions);
        }

        public bool CheckForCandleData()
        {
            return Directory.GetFiles(BacktesterDatabase.GetDataDirectory(), "*.db", SearchOption.TopDirectoryOnly).Count() != 0;
        }


        public async Task RefreshCandleData(List<string> coinsToRefresh, Action<string> callback, bool updateCandles, int period)
        {
             List<string> writtenFiles = new List<string>();

            foreach (var coinToBuy in coinsToRefresh)
            {
                DateTime startDate = Convert.ToDateTime(BacktestOptions.StartDate).ToUniversalTime();
                DateTime endDate = DateTime.UtcNow;
                var filePath = BacktesterDatabase.GetDataDirectory(BacktestOptions.Exchange.ToLower(), coinToBuy);

                LiteCollection<Candle> candleCollection = BacktesterDatabase.DataStore.GetInstance(filePath).GetTable<Candle>("Candle_" + period.ToString());

                // Delete an existing file if this is no update
                if (!updateCandles)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
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
                writtenFiles.Add(filePath);
            }

            // Delete everything that's not refreshed if we are not in update mode
            if (!updateCandles)
            {
                foreach (FileInfo fi in new DirectoryInfo(BacktesterDatabase.GetDataDirectory()).EnumerateFiles())
                {
                    if (!writtenFiles.Contains(fi.FullName))
                    {
                        File.Delete(fi.FullName);
                    }
                }
            }

            }

        public static void GetCacheAge()
        {
            Console.WriteLine("\tBacktest StartDate: " + Convert.ToDateTime(BacktestOptions.StartDate).ToUniversalTime().ToString() + " UTC");
            if (BacktestOptions.EndDate != null && BacktestOptions.EndDate != "")
            {
                Console.WriteLine("\tBacktest EndDate: " + Convert.ToDateTime(BacktestOptions.EndDate).ToUniversalTime().ToString() + " UTC");
            } else
            {
                Console.WriteLine("\tBacktest EndDate: " + DateTime.UtcNow.ToString() + " UTC");
            }

            Console.WriteLine("");

            int dataCount = 0;
            foreach (var coin in BacktestOptions.Coins)
            {
                string instance = BacktesterDatabase.GetDataDirectory() + "/" + BacktestOptions.Exchange.ToLower() + "_" + coin + ".db";
                if (File.Exists(instance))
                {
                    LiteCollection<Candle> getCacheAge = BacktesterDatabase.DataStore.GetInstance(instance).GetTable<Candle>("Candle_" + BacktestOptions.CandlePeriod);
                    Candle currentHistoricalDataLast = getCacheAge.Find(Query.All("Timestamp", Query.Descending), limit: 1).FirstOrDefault();
                    Candle currentHistoricalDataFirst = getCacheAge.Find(Query.All("Timestamp", Query.Ascending), limit: 1).FirstOrDefault();
                    Console.WriteLine("\tAvailable Cache for " + BacktestOptions.Exchange + " " + coin + " Period: " + BacktestOptions.CandlePeriod + "min  - from " + currentHistoricalDataFirst.Timestamp.ToUniversalTime() + " until " + currentHistoricalDataLast.Timestamp.ToUniversalTime());
                    dataCount = dataCount + 1;
                }
            }

            if (dataCount == 0)
            {
                Console.WriteLine("\tNo data - Please run 4. Refresh candle data first");
            }

            Console.WriteLine("");
        }
    }
}
