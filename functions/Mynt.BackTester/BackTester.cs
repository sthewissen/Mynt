using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mynt.BackTester.Traits;
using Mynt.Core.Bittrex;
using Mynt.Core.Bittrex.Models;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;
using Mynt.Core.Strategies;
using Newtonsoft.Json;
using EmaCross = Mynt.Core.Strategies.EmaCross;

namespace Mynt.BackTester
{
    public class BackTester
    {
        #region trading variables

        // As soon as our profit dips below this percentage we sell.
        private const double stopLossPercentage = -0.05;

        private readonly List<(int Duration, double Profit)> returnOnInvestment = new List<ValueTuple<int, double>>()
        {
            // These values determine how much time we want to way for profits.
            new ValueTuple<int, double>(5, 0.03), // If the profit percentage is above 3% after 5 minutes, we sell
            new ValueTuple<int, double>(10, 0.02), // If the profit percentage is above 2% after 10 minutes, we sell
            new ValueTuple<int, double>(30, 0.015),  // If the profit percentage is above 1,5% after 30 minutes, we sell
            new ValueTuple<int, double>(45, 0.005),  // If the profit percentage is above 0,5% after 45 minutes, we sell
            new ValueTuple<int, double>(0, 0.05)  // If the profit percentage is above 5% we always sell
        };

        // These are the coins we're interested in. 
        // This is what we have some backtest data for. 
        // You can always add more backtest data by saving data from the Bittrex API.
        private readonly List<string> coinsToBuy;
            


        private readonly List<double> stopLossAnchors = new List<double>()
        {
            // Use these to anchor in your profits. As soon as one of these profit percentages
            // has been reached we adjust our stop loss to become that percentage. 
            // That way we theoretically lock in some profits and continue to ride an uptrend
            0.01, 0.02, 0.03, 0.05, 0.08, 0.13, 0.21
        };

        private readonly List<ITrait> traits = new List<ITrait>()
        {
            new Traits.Cci(),
            new Traits.Cmo(),
            new Traits.EmaCross(),
            new Traits.Mfi(),
            new Traits.Rsi(),
            new Traits.SmaCross()
        };

        private readonly List<ITradingStrategy> strategies;
        #endregion

        #region constructors

        public BackTester(List<ITradingStrategy> strategies, string coinsToBuyCsv)
        {
            // Create Data Folder
            if (!Directory.Exists(GetDataDirectory()))
                Directory.CreateDirectory(GetDataDirectory());

            coinsToBuy = coinsToBuyCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();            

            this.strategies = strategies;
        }

        #endregion

        #region backtesting

        private void BackTest(ITradingStrategy strategy)
        {
            var results = new List<BackTestResult>();

            // Go through our coinpairs and backtest them.
            foreach (var pair in coinsToBuy)
            {
                var dataString = File.ReadAllText($"Data/{pair}.json");


                // This creates a list of buy signals.
                var candles = JsonConvert.DeserializeObject<List<Core.Models.Candle>>(dataString);
                var trend = strategy.Prepare(candles);

                for (int i = 0; i < trend.Count; i++)
                {
                    if (trend[i] == 1)
                    {
                        // This is a buy signal
                        var trade = new Trade() { OpenRate = candles[i].Close, OpenDate = candles[i].Timestamp, Quantity = 1 };

                        // Calculate win/lose forwards from buy point
                        for (int j = i; j < trend.Count; j++)
                        {
                            // There are 2 ways we can sell, either through the strategy telling us to do so (-1)
                            // or because other conditions such as the ROI or stoploss tell us to.
                            if (trend[j] == -1 || ShouldSell(trade, candles[j].Close, candles[j].Timestamp) != SellType.None)
                            {
                                // Bittrex charges 0,25% transaction fee, so deduct that.
                                var currentProfit = 0.995 * ((candles[j].Close - trade.OpenRate) / trade.OpenRate);
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
            foreach (var pair in coinsToBuy)
            {
                Console.Write($"\t{pair.ToUpper()}:".PadRight(15, ' '));
                PrintResults(results.Where(x => x.Currency == pair).ToList());
            }

            Console.WriteLine();
            Console.Write("\tTOTAL:".PadRight(15, ' '));
            PrintResults(results);
            WriteSeparator();
        }

        private void BackTestAll()
        {

            Console.WriteLine();
            Console.WriteLine(
                $"\t=============== BACKTESTING REPORT ===============");
            Console.WriteLine();

            foreach (var strategy in strategies.OrderBy(x => x.Name))
            {
                try
                {
                    var results = new List<BackTestResult>();

                    foreach (var pair in coinsToBuy)
                    {
                        var dataString = File.ReadAllText($"Data/{pair}.json");

                        // This creates a list of buy signals.
                        var candles = JsonConvert.DeserializeObject<List<Core.Models.Candle>>(dataString);
                        var trend = strategy.Prepare(candles);

                        for (int i = 0; i < trend.Count; i++)
                        {
                            if (trend[i] == 1)
                            {
                                // This is a buy signal
                                var trade = new Trade()
                                {
                                    OpenRate = candles[i].Close,
                                    OpenDate = candles[i].Timestamp,
                                    Quantity = 1
                                };

                                // Calculate win/lose forwards from buy point
                                for (int j = i; j < trend.Count; j++)
                                {
                                    if (trend[j] == -1 || ShouldSell(trade, candles[j].Close, candles[j].Timestamp) != SellType.None)
                                    {
                                        var currentProfit = 0.995 * ((candles[j].Close - trade.OpenRate) / trade.OpenRate);
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
        
        private void BackTestCombinations()
        {

            Console.WriteLine();
            Console.WriteLine(
                $"\t=============== BACKTESTING REPORT ===============");
            Console.WriteLine();

            var stratResults = new List<StrategyResult>();

            foreach (var strategy1 in strategies.Where(x => x.Name.ToUpper() == "CCI RSI").OrderBy(x => x.Name))
            {
                foreach (var strategy2 in strategies.OrderBy(x => x.Name))
                {
                    try
                    {
                        var results = new List<BackTestResult>();

                        foreach (var pair in coinsToBuy)
                        {
                            var dataString = File.ReadAllText($"Data/{pair}.json");

                            // This creates a list of buy signals.
                            var candles = JsonConvert.DeserializeObject<List<Core.Models.Candle>>(dataString);
                            var trend1 = strategy1.Prepare(candles);                            
                            var trend2 = strategy2.Prepare(candles);

                            for (int i = 0; i < trend1.Count; i++)
                            {
                                if (trend1[i] == 1 && trend2[i] == 1)
                                {
                                    // This is a buy signal
                                    var trade = new Trade()
                                    {
                                        OpenRate = candles[i].Close,
                                        OpenDate = candles[i].Timestamp,
                                        Quantity = 1
                                    };

                                    // Calculate win/lose forwards from buy point
                                    for (int j = i; j < trend1.Count; j++)
                                    {
                                        if (trend1[j] == -1 || trend2[j] == -1 || ShouldSell(trade, candles[j].Close, candles[j].Timestamp) != SellType.None)
                                        {
                                            var currentProfit = 0.995 * ((candles[j].Close - trade.OpenRate) / trade.OpenRate);
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

                        Console.WriteLine($"\t{strategy1.Name} + {strategy2.Name} FINISHED");

                        if (results.Count == 0)
                        {
                            stratResults.Add(new StrategyResult()
                            {
                                Name = $"{strategy1.Name} + {strategy2.Name}",
                                TotalTrades = 0,
                                ProfitTrades = 0,
                                NonProfitTrades = 0,
                                AvgProfit = 0,
                                TotalProfit = 0,
                                AvgTime = 0,
                                Grade = 0
                            });
                        }
                        else
                        {
                            stratResults.Add(new StrategyResult()
                            {
                                Name = $"{strategy1.Name} + {strategy2.Name}",
                                TotalTrades = results.Count,
                                ProfitTrades = results.Count(x => x.Profit > 0),
                                NonProfitTrades = results.Count(x => x.Profit <= 0),
                                AvgProfit = results.Select(x => x.Profit).Average() * 100,
                                TotalProfit = results.Select(x => x.Profit).Sum(),
                                AvgTime = results.Select(x => x.Duration).Average() * 5,
                                Grade = 0 // (1/ results.Select(x => x.Profit).Sum()) + (1 - (1/ results.Select(x => x.Duration).Average() * 5)) + ((Convert.ToDouble(results.Count(x => x.Profit > 0))/ Convert.ToDouble(results.Count)))
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\t  {strategy1.Name} + {strategy2.Name}: " + "DNF");
                    }
                }
            }

            PrintResults(stratResults);
        }

        private void BackTestEntryExit()
        {

            Console.WriteLine();
            Console.WriteLine(
                $"\t=============== BACKTESTING REPORT ===============");
            Console.WriteLine();

            var stratResults = new List<StrategyResult>();

            foreach (var entryStrat in strategies.OrderBy(x => x.Name))
            {
                foreach (var exitStrat in strategies.OrderBy(x => x.Name))
                {
                    try
                    {
                        var results = new List<BackTestResult>();

                        foreach (var pair in coinsToBuy)
                        {
                            var dataString = File.ReadAllText($"Data/{pair}.json");

                            // This creates a list of buy signals.
                            var candles = JsonConvert.DeserializeObject<List<Core.Models.Candle>>(dataString);
                            var trend1 = entryStrat.Prepare(candles);
                            var trend2 = exitStrat.Prepare(candles);

                            for (int i = 0; i < trend1.Count; i++)
                            {
                                if (trend1[i] == 1)
                                {
                                    // This is a buy signal
                                    var trade = new Trade()
                                    {
                                        OpenRate = candles[i].Close,
                                        OpenDate = candles[i].Timestamp,
                                        Quantity = 1
                                    };

                                    // Calculate win/lose forwards from buy point
                                    for (int j = i; j < trend1.Count; j++)
                                    {
                                        if (trend2[j] == -1 || ShouldSell(trade, candles[j].Close, candles[j].Timestamp) != SellType.None)
                                        {
                                            var currentProfit = 0.995 * ((candles[j].Close - trade.OpenRate) / trade.OpenRate);
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

                        Console.WriteLine($"\t{entryStrat.Name} + {exitStrat.Name} FINISHED");

                        if (results.Count == 0)
                        {
                            stratResults.Add(new StrategyResult()
                            {
                                Name = $"{entryStrat.Name} + {exitStrat.Name}",
                                TotalTrades = 0,
                                ProfitTrades = 0,
                                NonProfitTrades = 0,
                                AvgProfit = 0,
                                TotalProfit = 0,
                                AvgTime = 0,
                                Grade = 0
                            });
                        }
                        else
                        {
                            stratResults.Add(new StrategyResult()
                            {
                                Name = $"{entryStrat.Name} + {exitStrat.Name}",
                                TotalTrades = results.Count,
                                ProfitTrades = results.Count(x => x.Profit > 0),
                                NonProfitTrades = results.Count(x => x.Profit <= 0),
                                AvgProfit = results.Select(x => x.Profit).Average() * 100,
                                TotalProfit = results.Select(x => x.Profit).Sum(),
                                AvgTime = results.Select(x => x.Duration).Average() * 5,
                                Grade = 0 // (1/ results.Select(x => x.Profit).Sum()) + (1 - (1/ results.Select(x => x.Duration).Average() * 5)) + ((Convert.ToDouble(results.Count(x => x.Profit > 0))/ Convert.ToDouble(results.Count)))
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\t  {entryStrat.Name} + {exitStrat.Name}: " + "DNF");
                    }
                }
            }

            PrintResults(stratResults);
        }

        private void BackTestTraits()
        {

            Console.WriteLine();
            Console.WriteLine(
                $"\t=============== BACKTESTING REPORT ===============");
            Console.WriteLine();

            var stratResults = new List<StrategyResult>();
            var stratName = String.Empty;
            
            for (int x = 0; x < (1 << 8); x++)
            {
                var useCci = (x & (1 << 0)) != 0;
                var useCmo = (x & (1 << 1)) != 0;
                var useEmaCross = (x & (1 << 2)) != 0;
                var useMfi = (x & (1 << 3)) != 0;
                var useRsi = (x & (1 << 4)) != 0;
                var useSmaCross = (x & (1 << 5)) != 0;
                var useAdx = (x & (1 << 6)) != 0;
                var useAo = (x & (1 << 7)) != 0;

                stratName = string.Empty;
                stratName += useCci ? "|CCI" : "";
                stratName += useMfi ? "|MFI" : "";
                stratName += useCmo ? "|CMO" : "";
                stratName += useRsi ? "|RSI" : "";
                stratName += useSmaCross ? "|SMA+" : "";
                stratName += useEmaCross ? "|EMA+" : "";
                stratName += useAdx ? "|ADX" : "";
                stratName += useAo ? "|AO" : "";
                stratName = stratName.Trim('|');

                try
                {
                    var results = new List<BackTestResult>();

                    foreach (var pair in coinsToBuy)
                    {
                        var dataString = File.ReadAllText($"Data/{pair}.json");

                        // This creates a list of buy signals.
                        var candles = JsonConvert.DeserializeObject<List<Core.Models.Candle>>(dataString);

                        var cci = new Traits.Cci().Create(candles);
                        var mfi = new Traits.Mfi().Create(candles);
                        var cmo = new Traits.Cmo().Create(candles);
                        var rsi = new Traits.Rsi().Create(candles);
                        var emacross = new Traits.EmaCross().Create(candles);
                        var smacross = new Traits.SmaCross().Create(candles);
                        var adx = new Traits.Adx().Create(candles);
                        var ao = new Traits.Ao().Create(candles);

                        for (int i = 0; i < candles.Count; i++)
                        {
                            if (((useCci && cci[i] == 1) || !useCci) &&
                                    ((useMfi && mfi[i] == 1) || !useMfi) &&
                                    ((useCmo && cmo[i] == 1) || !useCmo) &&
                                    ((useRsi && rsi[i] == 1) || !useRsi) &&
                                    ((useEmaCross && emacross[i] == 1) || !useEmaCross) &&
                                    ((useSmaCross && smacross[i] == 1) || !useSmaCross) &&
                                    ((useAdx && adx[i] == 1) || !useAdx) &&
                                    ((useAo && ao[i] == 1) || !useAo))
                            {
                                // This is a buy signal
                                var trade = new Trade()
                                {
                                    OpenRate = candles[i].Close,
                                    OpenDate = candles[i].Timestamp,
                                    Quantity = 1
                                };

                                // Calculate win/lose forwards from buy point
                                for (int j = i; j < candles.Count; j++)
                                {
                                    if (((useCci && cci[i] == -1) || !useCci) &&
                                    ((useMfi && mfi[i] == -1) || !useMfi) &&
                                    ((useCmo && cmo[i] == -1) || !useCmo) &&
                                    ((useRsi && rsi[i] == -1) || !useRsi) &&
                                    ((useEmaCross && emacross[i] == -1) || !useEmaCross) &&
                                    ((useSmaCross && smacross[i] == -1) || !useSmaCross) &&
                                    ((useAdx && adx[i] == -1) || !useAdx) &&
                                    ((useAo && ao[i] == -1) || !useAo) || ShouldSell(trade, candles[j].Close, candles[j].Timestamp) != SellType.None)
                                    {
                                        var currentProfit = 0.995 * ((candles[j].Close - trade.OpenRate) / trade.OpenRate);
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
                    
                    Console.WriteLine($"\t{stratName} FINISHED");

                    if (results.Count == 0)
                    {
                        stratResults.Add(new StrategyResult()
                        {
                            Name = $"{stratName}",
                            TotalTrades = 0,
                            ProfitTrades = 0,
                            NonProfitTrades = 0,
                            AvgProfit = 0,
                            TotalProfit = 0,
                            AvgTime = 0,
                        });
                    }
                    else
                    {
                        stratResults.Add(new StrategyResult()
                        {
                            Name = $"{stratName}",
                            TotalTrades = results.Count,
                            ProfitTrades = results.Count(y => y.Profit > 0),
                            NonProfitTrades = results.Count(y => y.Profit <= 0),
                            AvgProfit = results.Select(y => y.Profit).Average() * 100,
                            TotalProfit = results.Select(y => y.Profit).Sum(),
                            AvgTime = results.Select(y => y.Duration).Average() * 5,
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\t  {stratName}: " + "DNF");
                }
            }


            PrintResults(stratResults);
        }

        private SellType ShouldSell(Trade trade, double currentRateBid, DateTime utcNow)
        {
            var currentProfit = (currentRateBid - trade.OpenRate) / trade.OpenRate;

            if (currentProfit < stopLossPercentage)
                return SellType.StopLoss;

            if (currentProfit < trade.StopLossAnchor)
                return SellType.StopLossAnchor;

            // Set a stop loss anchor to minimize losses.
            foreach (var item in stopLossAnchors)
            {
                if (currentProfit > item)
                    trade.StopLossAnchor = item - 0.01;
            }

            // Check if time matches and current rate is above threshold
            foreach (var item in returnOnInvestment)
            {
                var timeDiff = (utcNow - trade.OpenDate).TotalSeconds / 60;

                if (timeDiff > item.Duration && currentProfit > item.Profit)
                    return SellType.Timed;
            }

            return SellType.None;
        }

        #endregion

        #region results

        private void PrintResults(List<StrategyResult> results)
        {

            WriteSeparator();

            Console.WriteLine(results.OrderByDescending(x => x.TotalProfit).ToStringTable(new[] { "Name", "Total #", "Profitable #", "Nonprofit #", "Profit %", "Profit", "Avg profit", "Avg time" }, a => a.Name, a => a.TotalTrades,
                a => a.ProfitTrades, a => a.NonProfitTrades, a => a.TotalTrades > 0 ? ((Convert.ToDouble(a.ProfitTrades) / Convert.ToDouble(a.TotalTrades)) * 100.0).ToString("0.00") + "%" : "0%", a => a.TotalProfit.ToString("0.000"), a => a.AvgProfit.ToString("0.0"), a => a.AvgTime.ToString("0.0")));


            WriteSeparator();
        }

        private void PrintResults(List<BackTestResult> results)
        {
            var color = results.Select(x => x.Profit).Sum() > 0 ? ConsoleColor.Green : ConsoleColor.Red;

            if (results.Count > 0)
                WriteColoredLine(
                    $"{(Convert.ToDouble(results.Count(x => x.Profit > 0)) / Convert.ToDouble(results.Count) * 100.0):0.00}%  |  " +
                    $"Made {results.Count} buys ({results.Count(x => x.Profit > 0)}/{results.Count(x => x.Profit < 0)}). " +
                    $"Average profit {(results.Select(x => x.Profit).Average() * 100):0.00}%. " +
                    $"Total profit was {(results.Select(x => x.Profit).Sum()):0.000}. " +
                    $"Average duration {(results.Select(x => x.Duration).Average() * 5):0.0} mins.", color);
            else
                WriteColoredLine($"Made {results.Count} buys. ", ConsoleColor.Yellow);
        }
        
        #endregion

        #region console bootstrapping

        public void PresentMenuToUser()
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
                        var strats = strategies.OrderBy(x => x.Name).ToList();

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
                        Console.WriteLine("\tBacktesting all strategies takes some time and uses historic data. Starting...");
                        BackTestCombinations();
                        continue;
                    case "4":
                        Console.WriteLine("\tBacktesting all strategies takes some time and uses historic data. Starting...");
                        BackTestEntryExit();
                        continue;
                    case "5":
                        Console.WriteLine("\tBacktesting all traits. Starting...");
                        BackTestTraits();
                        continue;
                    case "6":
                        Console.WriteLine("\tRefreshing case. Starting...");
                        RefreshCandleData().Wait();
                        continue;
                    case "7":
                        Console.WriteLine("\tCopy example data. Starting...");
                        CopyExampleCandleData();
                        continue;
                    case "8":
                    default:
                        Environment.Exit(1);
                        break;
                }
                break;
            }
        }

        public void WriteIntro()
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

        public void WriteColoredLine(string line, ConsoleColor color, bool padded = false)
        {
            Console.ForegroundColor = color;
            if (padded) Console.WriteLine();
            Console.WriteLine(line);
            if (padded) Console.WriteLine();
            Console.ResetColor();
        }

        private void WriteMenu()
        {
            TimeSpan cacheAge = GetCacheAge();

            Console.WriteLine("\t1. Run a single strategy");
            Console.WriteLine("\t2. Run all strategies");
            Console.WriteLine("\t3. Combine 2 strategies");
            Console.WriteLine("\t4. Combine entry/exit strategies");
            Console.WriteLine("\t5. Combine traits");
            Console.WriteLine("\t6. Refresh candle data");
            Console.WriteLine("\t7. Copy example candle data");
            Console.WriteLine("\t8. Close the tool");
            Console.WriteLine();

            if (cacheAge == TimeSpan.MinValue)
                WriteColoredLine("\tCache is empty. You must refresh (6) or copy example data (7).", ConsoleColor.Red);
            else
                Console.WriteLine($"\tCache age: {cacheAge}"); // Ofcourse indivual files could differ in time


            Console.WriteLine();
            Console.Write("\tWhat do you want to do? ");
        }

        private void WriteColored(string line, ConsoleColor color, bool padded = false)
        {
            Console.ForegroundColor = color;
            if (padded) Console.WriteLine();
            Console.Write(line);
            if (padded) Console.WriteLine();
            Console.ResetColor();
        }

        private void WriteSeparator()
        {
            Console.WriteLine();
            Console.WriteLine("\t============================================================");
            Console.WriteLine();
        }

        private void ActionCompleted()
        {
            WriteColoredLine("\tCompleted...", ConsoleColor.DarkGreen);
            WriteColoredLine("\tPress key to continue...", ConsoleColor.Gray);
            Console.ReadKey();
            Console.Clear();
            WriteIntro();
            Console.WriteLine();
            Console.WriteLine();
        }

        #endregion

        #region data management

        static string GetDataDirectory()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(basePath, "Data");
        }

        static string GetJsonFilePath(string pair)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(GetDataDirectory(), $"{pair}.json");
        }

        public void CopyExampleCandleData()
        {
            string examplesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExampleData");
            string dataPath = GetDataDirectory();

            // Clear current data directory
            foreach (FileInfo fi in new DirectoryInfo(dataPath).EnumerateFiles())
                File.Delete(fi.FullName);

            // Copy all files
            foreach (FileInfo fi in new DirectoryInfo(examplesPath).EnumerateFiles())
                File.Copy(fi.FullName, Path.Combine(dataPath, fi.Name));

            // Finish up.
            ActionCompleted();
        }

        public async System.Threading.Tasks.Task RefreshCandleData()
        {
            DateTime startDate = DateTime.Now.AddDays(-30);
            var period = Core.Models.Period.FiveMinutes;

            BittrexApi api = new BittrexApi(true);

            List<string> writtenFiles = new List<string>();

            foreach (var coinToBuy in coinsToBuy)
            {
                WriteColoredLine($"\tRefreshing {coinToBuy}", ConsoleColor.DarkGreen);

                var candles = await api.GetTickerHistory(coinToBuy, startDate, period);
                var jsonPath = GetJsonFilePath(coinToBuy);

                if (File.Exists(jsonPath))
                    File.Delete(jsonPath);

                try
                {
                    File.WriteAllText(jsonPath, JsonConvert.SerializeObject(candles));
                    WriteColoredLine($"\tWritten {coinToBuy}", ConsoleColor.DarkGreen);
                    writtenFiles.Add(jsonPath);
                }
                catch (Exception e)
                {
                    WriteColoredLine($"\tRefreshCache failed for {coinToBuy}: {e.Message}", ConsoleColor.Red);
                }
            }

            // Delete everything that's not refreshed
            foreach (FileInfo fi in new DirectoryInfo(GetDataDirectory()).EnumerateFiles())
            {
                if (!writtenFiles.Contains(fi.FullName))
                    File.Delete(fi.FullName);
            }

            // Finish up.
            ActionCompleted();
        }

        static TimeSpan GetCacheAge()
        {
            string dataFolder = Path.GetDirectoryName(GetJsonFilePath("dummy-dummy"));

            if (Directory.GetFiles(dataFolder).Length == 0)
                return TimeSpan.MinValue;

            FileSystemInfo fileInfo = new DirectoryInfo(dataFolder).GetFileSystemInfos().OrderBy(fi => fi.CreationTime).First();

            return DateTime.Now - fileInfo.LastWriteTime;
        }

        #endregion
    }
}