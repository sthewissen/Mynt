using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExchangeSharp;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Extensions;
using Mynt.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mynt.Core.Backtester
{
    public class DataRefresher
    {
        public static bool CheckForCandleData()
        {
            return Directory.GetFiles(BacktesterDatabase.GetDataDirectory(), "*.db", SearchOption.TopDirectoryOnly).Count() != 0;
        }

        public static async Task RefreshCandleData(Action<string> callback, BacktestOptions backtestOptions)
        {

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true);
            IConfiguration Configuration = builder.Build();

            var exchangeOptions = Configuration.Get<ExchangeOptions>();
            Console.WriteLine(exchangeOptions);
            BaseExchange _api = new BaseExchange(exchangeOptions);

            List<string> writtenFiles = new List<string>();

            foreach (var coinToBuy in backtestOptions.Coins)
            {
                DateTime startDate = Convert.ToDateTime(backtestOptions.StartDate).ToUniversalTime();
                DateTime endDate = DateTime.UtcNow;
                var filePath = BacktesterDatabase.GetDataDirectory(backtestOptions.Exchange.ToLower(), coinToBuy);

                LiteCollection<Candle> candleCollection = BacktesterDatabase.DataStore.GetInstance(filePath).GetTable<Candle>("Candle_" + backtestOptions.CandlePeriod.ToString());

                // Delete an existing file if this is no update
                if (!backtestOptions.UpdateCandles)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    } 
                    callback($"Recreate database with Period {backtestOptions.CandlePeriod.ToString()}min for {coinToBuy.ToString()} from {startDate.ToString()} UTC to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                }
                else
                {
                    candleCollection.EnsureIndex("Timestamp");
                    Candle currentHistoricalData = candleCollection.Find(Query.All("Timestamp", Query.Descending), limit: 1).FirstOrDefault();
                    if (currentHistoricalData != null)
                    {
                        startDate = currentHistoricalData.Timestamp.ToUniversalTime();
                        callback($"Update database with Period {backtestOptions.CandlePeriod.ToString()}min for {coinToBuy.ToString()} from {startDate.ToString()} UTC to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                    } else
                    {
                        callback($"Create new database with Period {backtestOptions.CandlePeriod.ToString()}min for {coinToBuy.ToString()} from {startDate.ToString()} UTC to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                    }
                }

                // Get these in batches of 500 because they're limited in the API.
                while (startDate < endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod)))
                {
                    List<Candle> candles = await _api.GetTickerHistory(coinToBuy, backtestOptions.CandlePeriod.FromMinutesEquivalent(), startDate, endDate);
                    startDate = candles.LastOrDefault().Timestamp;
                    candleCollection.InsertBulk(candles);
                }

                callback($"Refreshed data for {coinToBuy}...");
                writtenFiles.Add(filePath);
            }

            // Delete everything that's not refreshed if we are not in update mode
            if (!backtestOptions.UpdateCandles)
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

        public static JArray GetCacheAge(BacktestOptions backtestOptions)
        {
            JArray jArrayResult = new JArray();

            foreach (var coin in backtestOptions.Coins)
            {
                string instance = BacktesterDatabase.GetDataDirectory() + "/" + backtestOptions.Exchange.ToLower() + "_" + coin + ".db";
                if (File.Exists(instance))
                {
                    LiteCollection<Candle> getCacheAge = BacktesterDatabase.DataStore.GetInstance(instance).GetTable<Candle>("Candle_" + backtestOptions.CandlePeriod);
                    Candle currentHistoricalDataLast = getCacheAge.Find(Query.All("Timestamp", Query.Descending), limit: 1).FirstOrDefault();
                    Candle currentHistoricalDataFirst = getCacheAge.Find(Query.All("Timestamp", Query.Ascending), limit: 1).FirstOrDefault();
                    JObject currentResult = new JObject();
                    currentResult["Exchange"] = coin;
                    currentResult["Coin"] = coin;
                    currentResult["CandlePeriod"] = backtestOptions.CandlePeriod;
                    currentResult["FirstCandleDate"] = currentHistoricalDataFirst.Timestamp.ToUniversalTime();
                    currentResult["LastCandleDate"] = currentHistoricalDataLast.Timestamp.ToUniversalTime();
                    jArrayResult.Add(currentResult);
                }
            }
            return jArrayResult;
        }

        public static void GetCacheAgeConsole(BacktestOptions backtestOptions)
        {
            Console.WriteLine("\tBacktest StartDate: " + Convert.ToDateTime(backtestOptions.StartDate).ToUniversalTime().ToString() + " UTC");
            if (backtestOptions.EndDate != null && backtestOptions.EndDate != "")
            {
                Console.WriteLine("\tBacktest EndDate: " + Convert.ToDateTime(backtestOptions.EndDate).ToUniversalTime().ToString() + " UTC");
            } else
            {
                Console.WriteLine("\tBacktest EndDate: " + DateTime.UtcNow.ToString() + " UTC");
            }

            Console.WriteLine("");

            int dataCount = 0;
            foreach (var coin in backtestOptions.Coins)
            {
                string instance = BacktesterDatabase.GetDataDirectory() + "/" + backtestOptions.Exchange.ToLower() + "_" + coin + ".db";
                if (File.Exists(instance))
                {
                    LiteCollection<Candle> getCacheAge = BacktesterDatabase.DataStore.GetInstance(instance).GetTable<Candle>("Candle_" + backtestOptions.CandlePeriod);
                    Candle currentHistoricalDataLast = getCacheAge.Find(Query.All("Timestamp", Query.Descending), limit: 1).FirstOrDefault();
                    Candle currentHistoricalDataFirst = getCacheAge.Find(Query.All("Timestamp", Query.Ascending), limit: 1).FirstOrDefault();
                    Console.WriteLine("\tAvailable Cache for " + backtestOptions.Exchange + " " + coin + " Period: " + backtestOptions.CandlePeriod + "min  - from " + currentHistoricalDataFirst.Timestamp.ToUniversalTime() + " until " + currentHistoricalDataLast.Timestamp.ToUniversalTime());
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
