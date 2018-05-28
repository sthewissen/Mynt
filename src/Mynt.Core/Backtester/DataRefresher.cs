using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mynt.Core.Exchanges;
using Mynt.Core.Extensions;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;
using Newtonsoft.Json.Linq;

namespace Mynt.Core.Backtester
{
    public class DataRefresher
    {
        public static Dictionary<string, BacktestOptions> CurrentlyRunningUpdates = new Dictionary<string, BacktestOptions>();

        public static async Task<bool> CheckForCandleData(BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            List<string> allDatabases = await dataStore.GetBacktestAllDatabases(backtestOptions);
            if (allDatabases.Count == 0)
            {
                return false;
            }
            return true;
        }

        public static async Task RefreshCandleData(Action<string> callback, BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            BaseExchange baseExchangeApi = new BaseExchangeInstance().BaseExchange(backtestOptions.Exchange.ToString());

            foreach (string globalSymbol in backtestOptions.Coins)
            {
                string exchangeSymbol = baseExchangeApi.GlobalSymbolToExchangeSymbol(globalSymbol);
                backtestOptions.Coin = globalSymbol;
                string currentlyRunningString = backtestOptions.Exchange + "_" + globalSymbol + "_" + backtestOptions.CandlePeriod;
                lock (CurrentlyRunningUpdates)
                {
                    if (CurrentlyRunningUpdates.ContainsKey(currentlyRunningString))
                    {
                        callback($"\tUpdate still in process:  {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {globalSymbol} from {backtestOptions.StartDate} UTC to {DateTime.UtcNow.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                        return;
                    }
                    CurrentlyRunningUpdates[currentlyRunningString] = backtestOptions;
                }

                DateTime startDate = Convert.ToDateTime(backtestOptions.StartDate).ToUniversalTime();
                DateTime endDate = DateTime.UtcNow;
                bool databaseExists = true;

                // Delete an existing file if this is no update
                if (!backtestOptions.UpdateCandles)
                {
                    dataStore.DeleteBacktestDatabase(backtestOptions).RunSynchronously();
					callback($"\tRecreate database: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {globalSymbol} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                }
                else
                {
                    //candleCollection.EnsureIndex("Timestamp");
                    Candle databaseLastCandle = await dataStore.GetBacktestLastCandle(backtestOptions);
                    if (databaseLastCandle != null)
                    {
                        startDate = databaseLastCandle.Timestamp.ToUniversalTime();
                        callback($"\tUpdate database: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {globalSymbol} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                    } else
                    {
						callback($"\tCreate database: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {globalSymbol} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                        databaseExists = false;
                    }
                }

                if (startDate == endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod)))
                {
                    callback($"\tAlready up to date: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {globalSymbol} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                    lock (CurrentlyRunningUpdates)
                    {
                        CurrentlyRunningUpdates.Remove(currentlyRunningString);
                    }
                    return;
                }

                // Get these in batches of 500 because they're limited in the API.
                while (startDate < endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod)))
                {
                    try
                    {
                        List<Candle> candles = await baseExchangeApi.GetTickerHistory(exchangeSymbol, backtestOptions.CandlePeriod.FromMinutesEquivalent(), startDate, endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod)));
                        if (candles.Count == 0 || candles.Last().Timestamp.ToUniversalTime() == startDate)
                        {
                            callback($"\tNo update: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {globalSymbol} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                            break;
                        }
                        startDate = candles.Last().Timestamp.ToUniversalTime();

                        if (!databaseExists)
                        {
                            await dataStore.SaveBacktestCandlesBulk(candles, backtestOptions);
                            databaseExists = true;
                        } else {
                            await dataStore.SaveBacktestCandlesBulkCheckExisting(candles, backtestOptions);
                        }

                        callback($"\tUpdated: {backtestOptions.Exchange.ToString()} with Period {backtestOptions.CandlePeriod.ToString()}min for {globalSymbol} {startDate.ToUniversalTime()} to {endDate.RoundDown(TimeSpan.FromMinutes(backtestOptions.CandlePeriod))} UTC");
                    }
                    catch (Exception e)
                    {
                        callback($"\tError while updating: {backtestOptions.Exchange.ToString()} {globalSymbol}: {e.Message}");
                        break;
                    }
                }
                lock (CurrentlyRunningUpdates)
                {
                    CurrentlyRunningUpdates.Remove(currentlyRunningString);
                }
            }
        }

        public static async Task<JArray> GetCacheAge(BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            JArray jArrayResult = new JArray();

            foreach (var globalSymbol in backtestOptions.Coins)
            {
                backtestOptions.Coin = globalSymbol;

                Candle currentHistoricalDataFirst = await dataStore.GetBacktestFirstCandle(backtestOptions);
                Candle currentHistoricalDataLast = await dataStore.GetBacktestLastCandle(backtestOptions);
                if (currentHistoricalDataFirst != null && currentHistoricalDataLast != null)
                {
                    JObject currentResult = new JObject();
                    currentResult["Exchange"] = backtestOptions.Exchange.ToString();
                    currentResult["Coin"] = globalSymbol;
                    currentResult["CandlePeriod"] = backtestOptions.CandlePeriod;
                    currentResult["FirstCandleDate"] = currentHistoricalDataFirst.Timestamp.ToUniversalTime();
                    currentResult["LastCandleDate"] = currentHistoricalDataLast.Timestamp.ToUniversalTime();
                    jArrayResult.Add(currentResult);
                }
            }
            return jArrayResult;
        }

        public static void GetCacheAgeConsole(BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            Console.WriteLine("\tBacktest StartDate: " + Convert.ToDateTime(backtestOptions.StartDate).ToUniversalTime() + " UTC");
            if (backtestOptions.EndDate != DateTime.MinValue)
            {
                Console.WriteLine("\tBacktest EndDate: " + Convert.ToDateTime(backtestOptions.EndDate).ToUniversalTime() + " UTC");
            } else
            {
                Console.WriteLine("\tBacktest EndDate: " + DateTime.UtcNow + " UTC");
            }

            Console.WriteLine("");

            int dataCount = 0;
            foreach (var globalSymbol in backtestOptions.Coins)
            {
                backtestOptions.Coin = globalSymbol;

                Candle currentHistoricalDataFirst = dataStore.GetBacktestFirstCandle(backtestOptions).Result;
                Candle currentHistoricalDataLast = dataStore.GetBacktestLastCandle(backtestOptions).Result;
                if (currentHistoricalDataFirst != null && currentHistoricalDataLast != null)
                {
                    Console.WriteLine("\tAvailable Cache for " + backtestOptions.Exchange + " " + globalSymbol + " Period: " + backtestOptions.CandlePeriod + "min  - from " + currentHistoricalDataFirst.Timestamp.ToUniversalTime() + " until " + currentHistoricalDataLast.Timestamp.ToUniversalTime());
                    dataCount = dataCount + 1;
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
