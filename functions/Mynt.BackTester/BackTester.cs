using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mynt.Core.Api;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;
using Mynt.DataAccess.FileBasedStorage;
using Mynt.DataAccess.Interfaces;
using Newtonsoft.Json;

namespace Mynt.BackTester
{
    public class BackTester
    {
        #region trading variables

        private IDataStorage dataStorage;

        private IExchangeApi exchangeApi;

        // As soon as our profit dips below this percentage we sell.
        private const double stopLossPercentage = -0.1;

        private readonly List<(int Duration, double Profit)> returnOnInvestment = new List<ValueTuple<int, double>>()
        {
            // These values determine how much time we want to way for profits.
            //new ValueTuple<int, double>(5, 0.03), // If the profit percentage is above 3% after 5 minutes, we sell
            //new ValueTuple<int, double>(10, 0.02), // If the profit percentage is above 2% after 10 minutes, we sell
            //new ValueTuple<int, double>(30, 0.015),  // If the profit percentage is above 1,5% after 30 minutes, we sell
            //new ValueTuple<int, double>(45, 0.005),  // If the profit percentage is above 0,5% after 45 minutes, we sell
            new ValueTuple<int, double>(0, 0.02)  // If the profit percentage is above 5% we always sell
        };

        // These are the coins we're interested in. 
        // This is what we have some backtest data for. 
        // You can always add more backtest data by saving data from the Bittrex API.
        private readonly IEnumerable<string> coinsToBuy;

        private readonly List<double> stopLossAnchors = new List<double>()
        {
            // Use these to anchor in your profits. As soon as one of these profit percentages
            // has been reached we adjust our stop loss to become that percentage. 
            // That way we theoretically lock in some profits and continue to ride an uptrend
            // 0.01, 0.02, 0.03, 0.05, 0.08, 0.13, 0.21
        };

        private readonly List<ITradingStrategy> strategies;
        #endregion

        #region constructors

        public BackTester(List<ITradingStrategy> strategies, IExchangeApi exchangeApi, IDataStorage dataStorage, string coinsToBuyCsv) :
            this(strategies, exchangeApi, dataStorage, coinsToBuyCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList())
        {

        }

        public BackTester(List<ITradingStrategy> strategies, IExchangeApi exchangeApi, IDataStorage dataStorage, IEnumerable<string> coinsToBuy)
        {
            // Create Data Folder
            if (!Directory.Exists(GetDataDirectory()))
                Directory.CreateDirectory(GetDataDirectory());

            this.coinsToBuy = coinsToBuy;

            this.strategies = strategies;
            this.exchangeApi = exchangeApi;
            this.dataStorage = dataStorage;
        }

        #endregion

        #region backtesting

        private void BackTest(ITradingStrategy strategy)
        {
            List<BackTestResult> results = RunBackTest(strategy);

            Console.WriteLine();
            Console.WriteLine($"\t=============== BACKTESTING REPORT {strategy.Name} ===============");
            Console.WriteLine();

            // Prints the results for each coin for this strategy.
            foreach (var pair in coinsToBuy)
            {
                Console.Write($"\t{pair.ToUpper()}:".PadRight(15, ' '));
                PrintResults(results.Where(x => x.Currency == pair).ToList());
                dataStorage.Save<BackTestResult>($"{strategy.Name}-{pair}", results.Where(x => x.Currency == pair));
            }

            Console.WriteLine();
            Console.Write("\tTOTAL:".PadRight(15, ' '));
            PrintResults(results);
            dataStorage.Save<BackTestResult>($"{strategy.Name}-Total", results);
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
                    List<BackTestResult> results = RunBackTest(strategy);
                    Console.Write($"\t{strategy.Name}:".PadRight(35, ' '));
                    PrintResults(results);
                    dataStorage.Save<BackTestResult>($"{strategy.Name}-summary", results);
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
                                if (trend1[i].TradeAdvice == TradeAdvice.Buy && trend2[i].TradeAdvice == TradeAdvice.Buy)
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
                                        if (trend1[j].TradeAdvice == TradeAdvice.Sell || trend2[j].TradeAdvice == TradeAdvice.Sell || ShouldSell(trade, candles[j].Close, candles[j].Timestamp) != SellType.None)
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
                                if (trend1[i].TradeAdvice == TradeAdvice.Buy)
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
                                        if (trend2[j].TradeAdvice == TradeAdvice.Sell || ShouldSell(trade, candles[j].Close, candles[j].Timestamp) != SellType.None)
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

        private List<BackTestResult> RunBackTest(ITradingStrategy strategy)
        {
            var results = new List<BackTestResult>();

            // Go through our coinpairs and backtest them.
            foreach (var pair in coinsToBuy)
            {
                var candleProvider = new JsonCandleProvider("Data");

                // This creates a list of buy signals.
                var candles = candleProvider.GetCandles(pair);
                var trend = strategy.Prepare(candles);
                dataStorage.Save<ITradeAdvice>($"{strategy.Name}-{pair}", trend);

                for (int i = 0; i < trend.Count; i++)
                {
                    if (trend[i].TradeAdvice == TradeAdvice.Buy)
                    {
                        // This is a buy signal
                        var trade = new Trade() { OpenRate = candles[i].Close, OpenDate = candles[i].Timestamp, Quantity = 1 };

                        var buyStep = i;

                        // Calculate win/lose forwards from buy point
                        for (; i < trend.Count; i++)
                        {
                            // There are 2 ways we can sell, either through the strategy telling us to do so (-1)
                            // or because other conditions such as the ROI or stoploss tell us to.
                            if (trend[i].TradeAdvice == TradeAdvice.Sell || i == trend.Count - 1
                                || ShouldSell(trade, candles[i].Close, candles[i].Timestamp) != SellType.None)
                            {
                                // Bittrex charges 0.25% transaction fee
                                // Binance charges 0.1% transaction fee, 0.05% if paid in BNB
                                var currentProfit = (candles[i].Close - trade.OpenRate) / trade.OpenRate - 0.001;
                                results.Add(new BackTestResult { Currency = pair, Profit = currentProfit, Duration = i - buyStep });
                                break;
                            }
                        }
                    }
                }
            }

            return results;
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
            {
                var cumulativeProfits = results.GroupBy(_ => _.Currency).
                    Select(_ => (_.Select(x => (1 + x.Profit)).Aggregate((a, x) => a * x) - 1) * 100);
                var cumulativeProfit = cumulativeProfits.Average();
                var resultString = $"{(Convert.ToDouble(results.Count(x => x.Profit > 0)) / Convert.ToDouble(results.Count) * 100.0):0.00}%  |  " +
                    $"Made {results.Count} buys ({results.Count(x => x.Profit > 0)}/{results.Count(x => x.Profit < 0)}). " +
                    $"Average profit {(results.Select(x => x.Profit).Average() * 100):0.00}%. " +
                    $"Total profit was {(results.Select(x => x.Profit).Sum()):0.000}. " +
                    $"Profit when reinvesting was {cumulativeProfit:0.00}%. " +
                    $"Average duration {(results.Select(x => x.Duration).Average() * 5):0.0} mins.";
                WriteColoredLine(resultString, color);
            }
            else
            {
                WriteColoredLine($"Made {results.Count} buys. ", ConsoleColor.Yellow);
            }
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
                        Console.WriteLine("\tRefreshing case. Starting...");
                        RefreshCandleData().Wait();
                        continue;
                    case "6":
                        Console.WriteLine("\tCopy example data. Starting...");
                        CopyExampleCandleData();
                        continue;
                    case "7":
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
            Console.WriteLine("\t5. Refresh candle data");
            Console.WriteLine("\t6. Copy example candle data");
            Console.WriteLine("\t7. Close the tool");
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
            DateTime startDate = DateTime.Now.AddMinutes(-5 * 6000);
            var period = Period.FiveMinutes;

            List<string> writtenFiles = new List<string>();

            foreach (var coinToBuy in coinsToBuy)
            {
                WriteColoredLine($"\tRefreshing {coinToBuy}", ConsoleColor.DarkGreen);

                var candles = await exchangeApi.GetTickerHistory(coinToBuy, startDate, period);
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