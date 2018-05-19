using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Mynt.Core.Exchanges;
using Mynt.Core.Extensions;
using Mynt.Core.Models;
using Newtonsoft.Json.Linq;

namespace Mynt.Core.Backtester
{
    public class DataRefresher
    {
        public static Dictionary<string, BacktestOptions> CurrentlyRunningUpdates = new Dictionary<string, BacktestOptions>();

        public static bool CheckForCandleData(BacktestOptions backtestOptions)
        {
            return Directory.GetFiles(BacktesterDatabase.GetDataDirectory(backtestOptions.DataFolder), "*.db", SearchOption.TopDirectoryOnly).Count() != 0;
        }

        public static async Task RefreshCandleData(Action<string> callback, BacktestOptions backtestOptions)
        {
            BaseExchange baseExchangeApi = new BaseExchangeInstance().BaseExchange(backtestOptions.Exchange.ToString());

            List<string> writtenFiles = new List<string>();

            foreach (var coinToBuy in backtestOptions.Coins)
            {
                string currentlyRunningString = backtestOptions.Exchange + "_" + coinToBuy + "_" + backtestOptions.CandlePeriod;
                lock (CurrentlyRunningUpdates)
                {
                    if (CurrentlyRunningUpdates.ContainsKey(currentlyRunningString))
                    {
                        callback($"\tUpdate still in process:  {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {coinToBuy} from {backtestOptions.StartDate} UTC to {DateTime.UtcNow.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                        return;
                    }
                    CurrentlyRunningUpdates[currentlyRunningString] = backtestOptions;
                }

                DateTime startDate = Convert.ToDateTime(backtestOptions.StartDate).ToUniversalTime();
                DateTime endDate = DateTime.UtcNow;
                var filePath = BacktesterDatabase.GetDataDirectory(backtestOptions.DataFolder, backtestOptions.Exchange.ToString().ToLower(), coinToBuy);
                bool databaseExists = true;

                LiteCollection<Candle> candleCollection = BacktesterDatabase.DataStore.GetInstance(filePath).GetTable<Candle>("Candle_" + backtestOptions.CandlePeriod);

                // Delete an existing file if this is no update
                if (!backtestOptions.UpdateCandles)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    } 
					callback($"\tRecreate database: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {coinToBuy} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                }
                else
                {
                    candleCollection.EnsureIndex("Timestamp");
                    Candle databaseLastCandle = candleCollection.Find(Query.All("Timestamp", Query.Descending), limit: 1).FirstOrDefault();
                    if (databaseLastCandle != null)
                    {
                        startDate = databaseLastCandle.Timestamp.ToUniversalTime();
                        callback($"\tUpdate database: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {coinToBuy} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                    } else
                    {
						callback($"\tCreate database: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {coinToBuy} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                        databaseExists = false;
                    }
                }

                if (startDate == endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod)))
                {
                    callback($"\tAlready up to date: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {coinToBuy} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                    lock (CurrentlyRunningUpdates)
                    {
                        CurrentlyRunningUpdates.Remove(currentlyRunningString);
                    }
                    return;
                }

                // Get these in batches of 500 because they're limited in the API.
                while (startDate < endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod)))
                {
                    candleCollection.EnsureIndex("Timestamp");

                    try
                    {
                        List<Candle> candles = await baseExchangeApi.GetTickerHistory(coinToBuy, backtestOptions.CandlePeriod.FromMinutesEquivalent(), startDate, endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod)));
                        if (candles.Count == 0 || candles.Last().Timestamp.ToUniversalTime() == startDate)
                        {
                            callback($"\tNo update: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {coinToBuy} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                            break;
                        }
                        startDate = candles.Last().Timestamp.ToUniversalTime();

                        if (!databaseExists)
                        {
                            candleCollection.InsertBulk(candles);
                            databaseExists = true;
                        } else {
                            foreach (var candle in candles)
                            {
                                var newCandle = candleCollection.FindOne(x => x.Timestamp == candle.Timestamp);
                                if (newCandle == null)
                                {
                                    candleCollection.Insert(candle);
                                }
                            }
                        }

                        callback($"\tUpdated: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {coinToBuy} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                    }
                    catch (Exception e)
                    {
                        callback($"\tError while updating: {backtestOptions.Exchange.ToString()} {coinToBuy}: {e.Message}");
                        break;
                    }
                }

                writtenFiles.Add(filePath);
                lock (CurrentlyRunningUpdates)
                {
                    CurrentlyRunningUpdates.Remove(currentlyRunningString);
                }
            }

            // Delete everything that's not refreshed if we are not in update mode
            if (!backtestOptions.UpdateCandles)
            {
                foreach (FileInfo fi in new DirectoryInfo(BacktesterDatabase.GetDataDirectory(backtestOptions.DataFolder)).EnumerateFiles())
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
				string instance = BacktesterDatabase.GetDataDirectory(backtestOptions.DataFolder) + "/" + backtestOptions.Exchange.ToString().ToLower() + "_" + coin + ".db";
                if (File.Exists(instance))
                {
                    LiteCollection<Candle> getCacheAge = BacktesterDatabase.DataStore.GetInstance(instance).GetTable<Candle>("Candle_" + backtestOptions.CandlePeriod);
                    Candle currentHistoricalDataLast = getCacheAge.Find(Query.All("Timestamp", Query.Descending), limit: 1).FirstOrDefault();
                    Candle currentHistoricalDataFirst = getCacheAge.Find(Query.All("Timestamp"), limit: 1).FirstOrDefault();
                    if (currentHistoricalDataFirst != null && currentHistoricalDataLast != null)
                    {
                        JObject currentResult = new JObject();
                        currentResult["Exchange"] = coin;
                        currentResult["Coin"] = coin;
                        currentResult["CandlePeriod"] = backtestOptions.CandlePeriod;
                        currentResult["FirstCandleDate"] = currentHistoricalDataFirst.Timestamp.ToUniversalTime();
                        currentResult["LastCandleDate"] = currentHistoricalDataLast.Timestamp.ToUniversalTime();
                        jArrayResult.Add(currentResult);
                    }
                }
            }
            return jArrayResult;
        }

        public static void GetCacheAgeConsole(BacktestOptions backtestOptions)
        {
            Console.WriteLine("\tBacktest StartDate: " + Convert.ToDateTime(backtestOptions.StartDate).ToUniversalTime() + " UTC");
            if (!string.IsNullOrEmpty(backtestOptions.EndDate))
            {
                Console.WriteLine("\tBacktest EndDate: " + Convert.ToDateTime(backtestOptions.EndDate).ToUniversalTime() + " UTC");
            } else
            {
                Console.WriteLine("\tBacktest EndDate: " + DateTime.UtcNow + " UTC");
            }

            Console.WriteLine("");

            int dataCount = 0;
            foreach (var coin in backtestOptions.Coins)
            {
				string instance = BacktesterDatabase.GetDataDirectory(backtestOptions.DataFolder) + "/" + backtestOptions.Exchange.ToString().ToLower() + "_" + coin + ".db";
                if (File.Exists(instance))
                {
                    LiteCollection<Candle> getCacheAge = BacktesterDatabase.DataStore.GetInstance(instance).GetTable<Candle>("Candle_" + backtestOptions.CandlePeriod);
                    Candle currentHistoricalDataLast = getCacheAge.Find(Query.All("Timestamp", Query.Descending), limit: 1).FirstOrDefault();
                    Candle currentHistoricalDataFirst = getCacheAge.Find(Query.All("Timestamp"), limit: 1).FirstOrDefault();
                    if (currentHistoricalDataFirst != null && currentHistoricalDataLast != null)
                    {
                        Console.WriteLine("\tAvailable Cache for " + backtestOptions.Exchange + " " + coin + " Period: " + backtestOptions.CandlePeriod + "min  - from " + currentHistoricalDataFirst.Timestamp.ToUniversalTime() + " until " + currentHistoricalDataLast.Timestamp.ToUniversalTime());
                        dataCount = dataCount + 1;
                    }
                }
            }

            if (dataCount == 0)
            {
                Console.WriteLine("\tNo data - Please run 4. Refresh candle data first");
            }

            Console.WriteLine();
        }
    }
}
