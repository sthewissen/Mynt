using Mynt.BackTester.Models;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;
using Mynt.Core.TradeManagers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mynt.BackTester
{
    public class BackTester
    {
        private readonly IExchangeApi _exchangeApi;
        private readonly TradeOptions _options;
        private readonly IEnumerable<string> _coinsToBuy;
        private readonly List<ITradingStrategy> _strategies;

        public BackTester(List<ITradingStrategy> strategies, IExchangeApi baseExchange, TradeOptions options, IEnumerable<string> coinsToBuy)
        {
            // Create Data Folder
            if (!Directory.Exists(GetDataDirectory()))
                Directory.CreateDirectory(GetDataDirectory());

            _coinsToBuy = coinsToBuy;
            _strategies = strategies;
            _exchangeApi = baseExchange;
            _options = options;
        }

        #region backtesting

        private void BackTest(ITradingStrategy strategy)
        {
            List<BackTestResult> results = RunBackTest(strategy);

            Console.WriteLine();
            Console.WriteLine($"\t=============== BACKTESTING REPORT {strategy.Name} ===============");
            Console.WriteLine();

            // Prints the results for each coin for this strategy.
            foreach (var pair in _coinsToBuy)
            {
                Console.Write($"\t{pair.ToUpper()}:".PadRight(15, ' '));
                PrintResults(results.Where(x => x.Currency == pair).ToList());
            }

            WriteSeparator();
        }

        private void BackTestAll()
        {
            Console.WriteLine();
            Console.WriteLine(
                $"\t=============== BACKTESTING REPORT ===============");
            Console.WriteLine();

            foreach (var strategy in _strategies.OrderBy(x => x.Name))
            {
                try
                {
                    List<BackTestResult> results = RunBackTest(strategy);
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

        private List<BackTestResult> RunBackTest(ITradingStrategy strategy)
        {
            var results = new List<BackTestResult>();

            // Go through our coinpairs and backtest them.
            foreach (var pair in _coinsToBuy)
            {
                var candleProvider = new JsonCandleProvider("Data");

                // This creates a list of buy signals.
                var candles = candleProvider.GetCandles(pair);
                var trend = strategy.Prepare(candles);

                for (int i = 0; i < trend.Count; i++)
                {
                    if (trend[i] == TradeAdvice.Buy)
                    {
                        // This is a buy signal
                        var trade = new Trade() { OpenRate = candles[i].Close, OpenDate = candles[i].Timestamp, Quantity = 1 };

                        var buyStep = i;

                        // Calculate win/lose forwards from buy point
                        for (; i < trend.Count; i++)
                        {
                            // There are 2 ways we can sell, either through the strategy telling us to do so (-1)
                            // or because other conditions such as the ROI or stoploss tell us to.
                            if (trend[i] == TradeAdvice.Sell || i == trend.Count - 1)
                            {
                                var currentProfit = (candles[i].Close - trade.OpenRate) / trade.OpenRate;
                                results.Add(new BackTestResult { Currency = pair, Profit = currentProfit, Duration = i - buyStep });
                                break;
                            }
                        }
                    }
                }
            }

            return results;
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
                var resultString = $"{(Convert.ToDouble(results.Count(x => x.Profit > 0)) / Convert.ToDouble(results.Count) * 100.0):0.00}%  |  " +
                    $"Made {results.Count} buys ({results.Count(x => x.Profit > 0)}/{results.Count(x => x.Profit < 0)}). " +
                    $"Average profit {(results.Select(x => x.Profit).Average() * 100):0.00}%. " +
                    $"Total profit was {(results.Select(x => x.Profit).Sum()):0.000}. " +
                    $"Average duration {(results.Select(x => x.Duration).Average() * 60):0.0} mins.";
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
                        var strats = _strategies.OrderBy(x => x.Name).ToList();

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
                        Console.WriteLine("\tRefreshing case. Starting...");
                        RefreshCandleData().Wait();
                        continue;
                    case "4":
                        Console.WriteLine("\tCopy example data. Starting...");
                        CopyExampleCandleData();
                        continue;
                    case "5":
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
            Console.WriteLine("\t3. Refresh candle data");
            Console.WriteLine("\t4. Copy example candle data");
            Console.WriteLine("\t5. Close the tool");
            Console.WriteLine();

            if (cacheAge == TimeSpan.MinValue)
                WriteColoredLine("\tCache is empty. You must refresh (3) or copy example data (4).", ConsoleColor.Red);
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
            string examplesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleData");
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
            var period = Period.Hour;

            List<string> writtenFiles = new List<string>();

            foreach (var coinToBuy in _coinsToBuy)
            {
                WriteColoredLine($"\tRefreshing {coinToBuy}", ConsoleColor.DarkGreen);

                var candles = await _exchangeApi.GetTickerHistory(coinToBuy, period, 6000);
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
