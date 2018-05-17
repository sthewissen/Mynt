using Mynt.Backtester.Models;
using Mynt.Core.Interfaces;
using Mynt.Core.Strategies;
using Mynt.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

        public static void BackTest(ITradingStrategy strategy)
        {
            var runner = new BackTestRunner();
            var results = runner.RunSingleStrategy(strategy, BacktestOptions.Coins, BacktestOptions.StakeAmount, BacktestOptions.OnlyStartNewTradesWhenSold);

            if (BacktestOptions.ConsoleMode)
            {
                Console.WriteLine();
                Console.WriteLine($"\t=============== BACKTESTING REPORT {strategy.Name.ToUpper()} ===============");
                Console.WriteLine();
                ConsoleUtility.WriteColoredLine($"\tNote: Profit is based on trading with 0.1 BTC each trade.", ConsoleColor.Cyan);
                Console.WriteLine();
            }

            // Prints the results for each coin for this strategy.
            if (results.Count > 0)
            {
                Console.WriteLine(results
                                  .OrderByDescending(x => x.SuccessRate)
                                  .ToList()
                                  .ToStringTable<BackTestResult>(new string[] { "Market", "# Trades", "# Profitable", "Success Rate", "BTC Profit", "Profit %", "Avg. Duration", "Period" },
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
                if (BacktestOptions.ConsoleMode)
                {
                    ConsoleUtility.WriteColoredLine("\tNo backtests results found...", ConsoleColor.Red);
                }
                    
            }

            if (BacktestOptions.ConsoleMode)
            {
                ConsoleUtility.WriteSeparator();
            }
            
        }

        public static void BackTestShowTrades(ITradingStrategy strategy)
        {
            var runner = new BackTestRunner();
            var results = runner.RunSingleStrategy(strategy, BacktestOptions.Coins, BacktestOptions.StakeAmount, BacktestOptions.OnlyStartNewTradesWhenSold);

            if (BacktestOptions.ConsoleMode)
            {
                Console.WriteLine();
                Console.WriteLine($"\t=============== BACKTESTING REPORT {strategy.Name.ToUpper()} ===============");
                Console.WriteLine();
                ConsoleUtility.WriteColoredLine($"\tNote: Profit is based on trading with 0.1 BTC each trade.", ConsoleColor.Cyan);
                Console.WriteLine();
            }

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
                                  .ToStringTable<BackTestTradeResult>(new string[] { "Market", "Open", "Close", "BTC Profit", "Profit %", "Duration", "Startdate", "Enddate" },
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
                if (BacktestOptions.ConsoleMode)
                {
                    ConsoleUtility.WriteColoredLine("\tNo backtests results found...", ConsoleColor.Red);
                }
            }
            if (BacktestOptions.ConsoleMode)
            {
                ConsoleUtility.WriteSeparator();
            }
            
        }

        public static void BackTestAll()
        {
            var runner = new BackTestRunner();

            if (BacktestOptions.ConsoleMode)
            {
                Console.WriteLine();
                Console.WriteLine($"\t=============== BACKTESTING REPORT ===============");
                Console.WriteLine();
                ConsoleUtility.WriteColoredLine($"\tNote: Profit is based on trading with 0.1 BTC each trade.", ConsoleColor.Cyan);
                Console.WriteLine();
            }

            var results = new List<BackTestStrategyResult>();

            foreach (var item in GetTradingStrategies())
            {
                var stratResult = new BackTestStrategyResult() { Strategy = item.Name };
                stratResult.Results.AddRange(runner.RunSingleStrategy(item, BacktestOptions.Coins, BacktestOptions.StakeAmount, BacktestOptions.OnlyStartNewTradesWhenSold));
                results.Add(stratResult);
            };

            // Prints the results for each coin for this strategy.
            if (results.Count > 0)
            {
                Console.WriteLine(results
                                  .OrderByDescending(x => x.SuccessRate)
                                  .ToList()
                                  .ToStringTable<BackTestStrategyResult>(new string[] { "Strategy", "# Trades", "# Profitable", "Success Rate", "BTC Profit", "Profit %", "Avg. Duration", "Max. Period" },
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
                if (BacktestOptions.ConsoleMode)
                {
                    ConsoleUtility.WriteColoredLine("\tNo backtests results found...", ConsoleColor.Red);
                }
            }
            if (BacktestOptions.ConsoleMode)
            {
                ConsoleUtility.WriteSeparator();
            }
        }

        #endregion



    }
}
