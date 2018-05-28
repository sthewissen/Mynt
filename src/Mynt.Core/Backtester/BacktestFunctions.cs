using Mynt.Backtester.Models;
using Mynt.Core.Interfaces;
using Mynt.Core.Strategies;
using Mynt.Core.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Mynt.Core.Backtester
{
    public class BacktestFunctions
    {
        public static List<ITradingStrategy> GetTradingStrategies()
        {
            // Use reflection to get all the instances of our strategies.
            var strategyTypes = Assembly.GetAssembly(typeof(BaseStrategy)).GetTypes()
                                     .Where(type => type.IsSubclassOf(typeof(BaseStrategy)))
                                     .ToList();

            var strategies = new List<ITradingStrategy>();

            foreach (var item in strategyTypes)
            {
                strategies.Add((ITradingStrategy)Activator.CreateInstance(item));
            }

            return strategies;
        }

        #region backtesting

        public static async Task<List<BackTestResult>> BackTest(ITradingStrategy strategy, BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            var runner = new BackTestRunner();
            var results = await runner.RunSingleStrategy(strategy, backtestOptions, dataStore);
            return results;
        }

        public static async Task<JArray> BackTestJson(ITradingStrategy strategy, BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            List<BackTestResult> results = await BackTest(strategy, backtestOptions, dataStore);
            JArray jArrayResult = new JArray();

            if (results.Count > 0)
            {
                foreach (var result in results)
                {
                    JObject currentResult = new JObject();
                    currentResult["Strategy"] = strategy.Name;
                    currentResult["AmountOfTrades"] = result.AmountOfTrades;
                    currentResult["AmountOfProfitableTrades"] = result.AmountOfProfitableTrades;
                    currentResult["SuccessRate"] = result.SuccessRate;
                    currentResult["TotalProfit"] = result.TotalProfit;
                    currentResult["TotalProfitPercentage"] = result.TotalProfitPercentage;
                    currentResult["AverageDuration"] = result.AverageDuration;
                    currentResult["DataPeriod"] = result.DataPeriod;
                    jArrayResult.Add(currentResult);
                }
            }
            return jArrayResult;
        }

        public static async void BackTestConsole(ITradingStrategy strategy, BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            List<BackTestResult> results = await BackTest(strategy, backtestOptions, dataStore);
            if (results.Count > 0)
            {
                Console.WriteLine(results
                                  .OrderByDescending(x => x.SuccessRate)
                                  .ToList()
                                  .ToStringTable(new[] { "Market", "# Trades", "# Profitable", "Success Rate", "BTC Profit", "Profit %", "Avg. Duration", "Period" },
                                                                 (x) => x.Market,
                                                                 (x) => x.AmountOfTrades,
                                                                 (x) => x.AmountOfProfitableTrades,
                                                                 (x) => $"{x.SuccessRate:0.00}%",
                                                                 (x) => $"{x.TotalProfit:0.00000000}",
                                                                 (x) => $"{x.TotalProfitPercentage:0.00}%",
                                                                 (x) => $"{(x.AverageDuration):0.00} hours",
                                                                 (x) => $"{x.DataPeriod} days"));
            }
            else
            {
                ConsoleUtility.WriteColoredLine("\tNo backtests results found...", ConsoleColor.Red);
            }
            ConsoleUtility.WriteSeparator();
        }

        public static async Task<List<BackTestResult>> BackTestShowTrades(ITradingStrategy strategy, BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            var runner = new BackTestRunner();
            var results = await runner.RunSingleStrategy(strategy, backtestOptions, dataStore);
            return results;
        }

        public static async Task<JArray> BackTestShowTradesJson(ITradingStrategy strategy, BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            var results = await BackTestShowTrades(strategy, backtestOptions, dataStore);

            JArray jArrayResult = new JArray();

            var trades = new List<BackTestTradeResult>();

            foreach (var result in results)
            {
                trades.AddRange(result.Trades);
            }

            foreach (var trade in trades)
            {
                JObject currentResult = new JObject();
                currentResult["Strategy"] = strategy.Name;
                currentResult["Market"] = trade.Market;
                currentResult["OpenRate"] = trade.OpenRate;
                currentResult["CloseRate"] = trade.CloseRate;
                currentResult["Profit"] = trade.Profit;
                currentResult["ProfitPercentage"] = trade.ProfitPercentage;
                currentResult["Duration"] = trade.Duration;
                currentResult["StartDate"] = trade.StartDate;
                currentResult["EndDate"] = trade.EndDate;
                jArrayResult.Add(currentResult);
            }
            return jArrayResult;
        }

        public static async void BackTestShowTradesConsole(ITradingStrategy strategy, BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            var results = await BackTestShowTrades(strategy, backtestOptions, dataStore);

            Console.WriteLine();
            Console.WriteLine($"\t=============== BACKTESTING REPORT {strategy.Name.ToUpper()} ===============");
            Console.WriteLine();
            ConsoleUtility.WriteColoredLine($"\tNote: Profit is based on trading with 0.1 BTC each trade.", ConsoleColor.Cyan);
            Console.WriteLine();

            // Prints the results for each coin for this strategy.

            if (results.Count > 0)
            {
                var trades = new List<BackTestTradeResult>();

                foreach (var result in results)
                {
                    trades.AddRange(result.Trades);
                }

                Console.WriteLine(trades
                                  .OrderBy(x => x.StartDate)
                                  .ToList()
                                  .ToStringTable(new[] { "Market", "Open", "Close", "BTC Profit", "Profit %", "Duration", "Startdate", "Enddate" },
                                                                 (x) => x.Market,
                                                                 (x) => $"{x.OpenRate:0.00000000}",
                                                                 (x) => $"{x.CloseRate:0.00000000}",
                                                                 (x) => $"{x.Profit:0.00000000}",
                                                                 (x) => $"{x.ProfitPercentage:0.00}%",
                                                                 (x) => $"{(x.Duration):0.00} hours",
                                                                 (x) => $"{x.StartDate:dd-MM-yyyy hh:mm}",
                                                                 (x) => $"{x.EndDate:dd-MM-yyyy hh:mm}"));
            }
            else
            {
                ConsoleUtility.WriteColoredLine("\tNo backtests results found...", ConsoleColor.Red);
            }
            ConsoleUtility.WriteSeparator();
        }


        public static async Task<List<BackTestStrategyResult>> BackTestAll(BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            var runner = new BackTestRunner();
            var results = new List<BackTestStrategyResult>();

            foreach (var item in GetTradingStrategies())
            {
                var stratResult = new BackTestStrategyResult() { Strategy = item.Name };
                stratResult.Results.AddRange(await runner.RunSingleStrategy(item, backtestOptions, dataStore));
                results.Add(stratResult);
            }

            return results;
        }

        public static async Task<JArray> BackTestAllJson(BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            List<BackTestStrategyResult> results = await BackTestAll(backtestOptions, dataStore);
            JArray jArrayResult = new JArray();

            foreach (var result in results)
            {
                JObject currentResult = new JObject();
                currentResult["Strategy"] = result.Strategy;
                currentResult["AmountOfTrades"] = result.AmountOfTrades;
                currentResult["AmountOfProfitableTrades"] = result.AmountOfProfitableTrades;
                currentResult["SuccessRate"] = result.SuccessRate;
                currentResult["TotalProfit"] = result.TotalProfit;
                currentResult["TotalProfitPercentage"] = result.TotalProfitPercentage;
                currentResult["AverageDuration"] = result.AverageDuration;
                currentResult["DataPeriod"] = result.DataPeriod;
                jArrayResult.Add(currentResult);
            }
            return jArrayResult;
        }


        public static async void BackTestAllConsole(BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {

            List<BackTestStrategyResult> results = await BackTestAll(backtestOptions, dataStore);

            Console.WriteLine();
            Console.WriteLine($"\t=============== BACKTESTING REPORT ===============");
            Console.WriteLine();
            ConsoleUtility.WriteColoredLine($"\tNote: Profit is based on trading with 0.1 BTC each trade.", ConsoleColor.Cyan);
            Console.WriteLine();

            // Prints the results for each coin for this strategy.
            if (results.Count > 0)
            {
                Console.WriteLine(results
                                  .OrderByDescending(x => x.SuccessRate)
                                  .ToList()
                                  .ToStringTable(new[] { "Strategy", "# Trades", "# Profitable", "Success Rate", "BTC Profit", "Profit %", "Avg. Duration", "Max. Period" },
                                                                       (x) => x.Strategy,
                                                                       (x) => x.AmountOfTrades,
                                                                       (x) => x.AmountOfProfitableTrades,
                                                                       (x) => $"{x.SuccessRate:0.00}%",
                                                                       (x) => $"{x.TotalProfit:0.00000000}",
                                                                       (x) => $"{x.TotalProfitPercentage:0.00}%",
                                                                       (x) => $"{(x.AverageDuration):0.00} hours",
                                                                       (x) => $"{x.DataPeriod} days"));
            }
            else
            {

                ConsoleUtility.WriteColoredLine("\tNo backtests results found...", ConsoleColor.Red);
            }
            ConsoleUtility.WriteSeparator();
        }

        #endregion
    }
}
