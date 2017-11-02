using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mynt.Core.Api.Bittrex;
using Mynt.Core.Api.Bittrex.Models;
using Mynt.Core.Models;
using Mynt.Core.Strategies;
using Newtonsoft.Json;

namespace Mynt.BackTester
{
    class Program
    {

        #region trading variables

        // As soon as our profit dips below this percentage we sell.
        public const double StopLossPercentage = -0.03;

        public static readonly List<(int Duration, double Profit)> ReturnOnInvestment = new List<ValueTuple<int, double>>()
        {
            // These values determine how much time we want to way for profits.
            new ValueTuple<int, double>(5, 0.03), // If the profit percentage is above 3% after 5 minutes, we sell
            new ValueTuple<int, double>(10, 0.02), // If the profit percentage is above 2% after 10 minutes, we sell
            new ValueTuple<int, double>(30, 0.015),  // If the profit percentage is above 1,5% after 30 minutes, we sell
            new ValueTuple<int, double>(45, 0.005),  // If the profit percentage is above 0,5% after 45 minutes, we sell
            new ValueTuple<int, double>(0, 0.05)  // If the profit percentage is above 5% we always sell
        };

        private static readonly List<string> CoinsToBuy = new List<string>() {
            // These are the coins we're interested in. 
            // This is what we have some backtest data for. 
            // You can always add more backtest data by saving data from the Bittrex API.
            "btc-eth", "btc-neo", "btc-omg", "btc-edg", "btc-pay",
             "btc-pivx", "btc-qtum", "btc-mtl", "btc-etc", "btc-ltc"
        };

        public static readonly List<double> StopLossAnchors = new List<double>()
        {
            // Use these to anchor in your profits. As soon as one of these profit percentages
            // has been reached we adjust our stop loss to become that percentage. 
            // That way we theoretically lock in some profits and continue to ride an uptrend
            // 0.02, 0.03, 0.05, 0.08, 0.13, 0.21
        };

        public static readonly List<ITradingStrategy> Strategies = new List<ITradingStrategy>()
        {
            // The strategies we want to backtest.
            new Wvf(),
            new WvfExtended(),
            new FreqTrade(),
            new SmaCrossover()
        };

        #endregion

        static void Main(string[] args)
        {
            try
            {
                WriteIntro();
                Console.WriteLine();
                Console.WriteLine();
                PresentMenuToUser();
            }
            catch (Exception ex)
            {
                WriteColoredLine($"\t{ex.Message}", ConsoleColor.Red);
                Console.ReadLine();
            }
        }

        #region backtesting

        private static void BackTest(ITradingStrategy strategy)
        {
            var results = new List<BackTestResult>();

            // Go through our coinpairs and backtest them.
            foreach (var pair in CoinsToBuy)
            {
                var dataString = File.ReadAllText($"Data/{pair}.json");
                var data = JsonConvert.DeserializeObject<ApiResult<List<Core.Api.Bittrex.Models.Candle>>>(dataString);

                // This creates a list of buy signals.
                strategy.Candles = data.Result.ToGenericCandles();
                var trend = strategy.Prepare();

                for (int i = 0; i < trend.Count; i++)
                {
                    if (trend[i] == 1)
                    {
                        // This is a buy signal
                        var trade = new Trade() { OpenRate = strategy.Candles[i].Close, OpenDate = strategy.Candles[i].Timestamp, Quantity = 1 };

                        // Calculate win/lose forwards from buy point
                        for (int j = i; j < trend.Count; j++)
                        {
                            // There are 2 ways we can sell, either through the strategy telling us to do so (-1)
                            // or because other conditions such as the ROI or stoploss tell us to.
                            if (trend[j] == -1 || ShouldSell(trade, strategy.Candles[j].Close, strategy.Candles[j].Timestamp) != SellType.None)
                            {
                                // Bittrex charges 0,25% transaction fee, so deduct that.
                                var currentProfit = 0.995 * ((strategy.Candles[j].Close - trade.OpenRate) / trade.OpenRate);
                                results.Add(new BackTestResult { Currency = pair, Profit = currentProfit, Duration = j - i });
                                break;
                            }
                        }
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"\t=============== BACKTESTING REPORT {strategy.Name} ===============");
            Console.WriteLine();

            // Prints the results for each coin for this strategy.
            foreach (var pair in CoinsToBuy)
            {
                Console.Write($"\t{pair.ToUpper()}:".PadRight(15, ' '));
                PrintResults(results.Where(x => x.Currency == pair).ToList());
            }

            Console.WriteLine();
            Console.Write("\tTOTAL:".PadRight(15, ' '));
            PrintResults(results);
            WriteSeparator();
        }

        private static void BackTestAll()
        {

            Console.WriteLine();
            Console.WriteLine(
                $"\t=============== BACKTESTING REPORT ===============");
            Console.WriteLine();

            foreach (var strategy in Strategies.OrderBy(x => x.Name))
            {
                try
                {
                    var results = new List<BackTestResult>();

                    foreach (var pair in CoinsToBuy)
                    {
                        var dataString = File.ReadAllText($"Data/{pair}.json");
                        var data = JsonConvert.DeserializeObject<ApiResult<List<Core.Api.Bittrex.Models.Candle>>>(dataString);

                        // This creates a list of buy signals.
                        strategy.Candles = data.Result.ToGenericCandles();
                        var trend = strategy.Prepare();

                        for (int i = 0; i < trend.Count; i++)
                        {
                            if (trend[i] == 1)
                            {
                                // This is a buy signal
                                var trade = new Trade()
                                {
                                    OpenRate = strategy.Candles[i].Close,
                                    OpenDate = strategy.Candles[i].Timestamp,
                                    Quantity = 1
                                };

                                // Calculate win/lose forwards from buy point
                                for (int j = i; j < trend.Count; j++)
                                {
                                    if (trend[j] == -1 || ShouldSell(trade, strategy.Candles[j].Close, strategy.Candles[j].Timestamp) != SellType.None)
                                    {
                                        var currentProfit = 0.995 * ((strategy.Candles[j].Close - trade.OpenRate) / trade.OpenRate);
                                        results.Add(new BackTestResult
                                        {
                                            Currency = pair,
                                            Profit = currentProfit,
                                            Duration = j - i
                                        });
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    Console.Write($"\t{strategy.Name}:".PadRight(35, ' '));
                    PrintResults(results);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\t {strategy.Name}:".PadRight(35, ' ') + "DNF");
                }
            }

            WriteSeparator();
        }

        private static SellType ShouldSell(Trade trade, double currentRateBid, DateTime utcNow)
        {
            var currentProfit = (currentRateBid - trade.OpenRate) / trade.OpenRate;

            if (currentProfit < StopLossPercentage)
                return SellType.StopLoss;

            if (currentProfit < trade.StopLossAnchor)
                return SellType.StopLossAnchor;

            // Set a stop loss anchor to minimize losses.
            foreach (var item in StopLossAnchors)
            {
                if (currentProfit > item)
                    trade.StopLossAnchor = item;
            }

            // Check if time matches and current rate is above threshold
            foreach (var item in ReturnOnInvestment)
            {
                var timeDiff = (utcNow - trade.OpenDate).TotalSeconds / 60;

                if (timeDiff > item.Duration && currentProfit > item.Profit)
                    return SellType.Timed;
            }

            return SellType.None;
        }

        #endregion

        #region results

        private static void PrintResults(List<BackTestResult> results)
        {
            var color = results.Select(x => x.Profit).Sum() > 0 ? ConsoleColor.Green : ConsoleColor.Red;

            if (results.Count > 0)
                WriteColoredLine($"Made {results.Count} buys ({results.Where(x => x.Profit > 0).Count()}/{results.Where(x => x.Profit < 0).Count()}). " +
                                 $"Average profit {(results.Select(x => x.Profit).Average() * 100):0.00}%. " +
                                 $"Total profit was {(results.Select(x => x.Profit).Sum()):0.000}. " +
                                 $"Average duration {(results.Select(x => x.Duration).Average() * 5):0.0} mins.", color);
            else
                WriteColoredLine($"Made {results.Count} buys. ", ConsoleColor.Yellow);
        }


        #endregion

        #region console bootstrapping

        private static void PresentMenuToUser()
        {
            while (true)
            {
                // Write our menu.
                WriteMenu();

                // Get the option the user picked.
                var result = Console.ReadLine();

                WriteSeparator();

                // Depending on the option, pick a menu item.
                switch (result)
                {
                    case "1":
                        var strats = Strategies.OrderBy(x => x.Name).ToList();

                        for (int i = 0; i < strats.Count; i++)
                        {
                            Console.WriteLine($"\t\t{i + 1}. {strats[i].Name}");
                        }
                        Console.WriteLine();
                        Console.Write("\tWhich strategy do you want to test? ");
                        var index = Convert.ToInt32(Console.ReadLine());

                        if (index - 1 < strats.Count)
                        {
                            Console.WriteLine("\tBacktesting a single strategy. Starting...");
                            BackTest(strats[index - 1]);
                        }
                        else
                        {
                            WriteColoredLine("\tInvalid strategy choice.", ConsoleColor.Red);
                            PresentMenuToUser();
                        }

                        continue;
                    case "2":
                        Console.WriteLine("\tBacktesting all strategies takes some time and uses historic data. Starting...");
                        BackTestAll();
                        continue;
                    case "3":
                    default:
                        Environment.Exit(1);
                        break;
                }
                break;
            }
        }

        private static void WriteMenu()
        {
            Console.WriteLine("\t1. Run a single strategy");
            Console.WriteLine("\t2. Run all strategies");
            Console.WriteLine("\t3. Close the tool");

            Console.WriteLine();
            Console.Write("\tWhat do you want to do? ");
        }

        private static void WriteIntro()
        {
            Console.WriteLine();

            Console.WriteLine(@"                                       $$\");
            Console.WriteLine(@"                                       $$ |");
            Console.WriteLine(@"    $$$$$$\$$$$\  $$\   $$\ $$$$$$$\ $$$$$$\");
            Console.WriteLine(@"    $$  _$$  _$$\ $$ |  $$ |$$  __$$\\_$$  _|");
            Console.WriteLine(@"    $$ / $$ / $$ |$$ |  $$ |$$ |  $$ | $$ |");
            Console.WriteLine(@"    $$ | $$ | $$ |$$ |  $$ |$$ |  $$ | $$ |$$\");
            Console.WriteLine(@"    $$ | $$ | $$ |\$$$$$$$ |$$ |  $$ | \$$$$  |");
            Console.WriteLine(@"    \__| \__| \__| \____$$ |\__|  \__|  \____/");
            Console.WriteLine(@"                  $$\   $$ |");
            Console.WriteLine(@"                  \$$$$$$  |");
            Console.WriteLine(@"                   \______/");
        }

        static void WriteColoredLine(string line, ConsoleColor color, bool padded = false)
        {
            Console.ForegroundColor = color;
            if (padded) Console.WriteLine();
            Console.WriteLine(line);
            if (padded) Console.WriteLine();
            Console.ResetColor();
        }

        static void WriteColored(string line, ConsoleColor color, bool padded = false)
        {
            Console.ForegroundColor = color;
            if (padded) Console.WriteLine();
            Console.Write(line);
            if (padded) Console.WriteLine();
            Console.ResetColor();
        }

        public static void WriteSeparator()
        {
            Console.WriteLine();
            Console.WriteLine("\t============================================================");
            Console.WriteLine();
        }

        #endregion
    }
}