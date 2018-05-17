using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Mynt.Backtester.Models;
using Mynt.Core.Backtester;
using Mynt.Core.Configuration;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Interfaces;
using Mynt.Core.Strategies;
using Mynt.Core.Utility;

namespace Mynt.Backtester
{
    class Program
    {
        private static BackTestRunner _backTester;
        public static DataRefresher _dataRefresher;

        public static IConfiguration Configuration { get; set; }

        public static BacktestOptions BacktestOptions { get; set; }


        static void Main(string[] args)
        {
            try
            {
                BacktestOptions.ConsoleMode = true;
                Init();

                WriteIntro();
                Console.WriteLine();
                Console.WriteLine();

                if (!_dataRefresher.CheckForCandleData())
                {
                    ConsoleUtility.WriteColoredLine("\tNo data present. Please retrieve data first.", ConsoleColor.Red);
                    Console.WriteLine();
                }

                PresentMenuToUser();
            }
            catch (Exception ex)
            {
                ConsoleUtility.WriteColoredLine($"\t{ex.Message}", ConsoleColor.Red);
                Console.ReadLine();
            }
        }

        private static void Init()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true);
            Configuration = builder.Build();

            var exchangeOptions = Configuration.Get<ExchangeOptions>();
            BacktestOptions = Configuration.Get<BacktestOptions>();

            _backTester = new BackTestRunner();
            _dataRefresher = new DataRefresher(exchangeOptions);
        }

        #region console bootstrapping

        public static void PresentMenuToUser()
        {
            while (true)
            {
                // Write our menu.
                WriteMenu();

                // Get the option the user picked.
                var result = Console.ReadLine();

                ConsoleUtility.WriteSeparator();

                var strats = BacktestFunctions.GetTradingStrategies().OrderBy(x => x.Name).ToList();

                // Depending on the option, pick a menu item.
                switch (result)
                {
                    case "1":
                        for (int i = 0; i < strats.Count; i++)
                        {
                            Console.WriteLine($"\t\t{i + 1}. {strats[i].Name}");
                        }
                        Console.WriteLine();
                        Console.Write("\tWhich strategy do you want to test? ");
                        var index = Convert.ToInt32(Console.ReadLine());

                        if (index - 1 < strats.Count)
                        {
                            Console.WriteLine();
                            Console.WriteLine("\tBacktesting a single strategy. Starting...");
                            BacktestFunctions.BackTest(strats[index - 1]);
                        }
                        else
                        {
                            ConsoleUtility.WriteColoredLine("\tInvalid strategy choice.", ConsoleColor.Red);
                            PresentMenuToUser();
                        }

                        continue;

                    case "2":
                        for (int i = 0; i < strats.Count; i++)
                        {
                            Console.WriteLine($"\t\t{i + 1}. {strats[i].Name}");
                        }
                        Console.WriteLine();
                        Console.Write("\tWhich strategy do you want to test? ");
                        var indexStrategy = Convert.ToInt32(Console.ReadLine());

                        if (indexStrategy - 1 < strats.Count)
                        {
                            Console.WriteLine();
                            Console.WriteLine("\tBacktesting a single strategy. Starting...");
                            BacktestFunctions.BackTestShowTrades(strats[indexStrategy - 1]);
                        }
                        else
                        {
                            ConsoleUtility.WriteColoredLine("\tInvalid strategy choice.", ConsoleColor.Red);
                            PresentMenuToUser();
                        }

                        continue;

                    case "3":
                        Console.WriteLine("\tBacktesting all strategies takes some time and uses historic data. Starting...");
                        BacktestFunctions.BackTestAll();
                        continue;

                    case "4":
                        Console.WriteLine("\tRefreshing...");
                        Program._dataRefresher.RefreshCandleData(BacktestOptions.Coins, (x) => ConsoleUtility.WriteColoredLine(x, ConsoleColor.Green), BacktestOptions.UpdateCandles, BacktestOptions.CandlePeriod).Wait();
                        ActionCompleted();
                        continue;

                    case "5":
                    default:
                        Environment.Exit(1);
                        break;
                }
                break;
            }
        }

        public static void WriteIntro()
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

        public static void WriteMenu()
        {
            DataRefresher.GetCacheAge();

            Console.WriteLine("\t1. Run a single strategy");
            Console.WriteLine("\t2. Run a single strategy (show all trades)");
            Console.WriteLine("\t3. Run all strategies");
            Console.WriteLine("\t4. Refresh candle data");
            Console.WriteLine("\t5. Close the tool");
            Console.WriteLine();

            Console.WriteLine();
            Console.Write("\tWhat do you want to do? ");
        }



        private static void ActionCompleted()
        {
            ConsoleUtility.WriteColoredLine("\tCompleted...", ConsoleColor.DarkGreen);
            ConsoleUtility.WriteColoredLine("\tPress key to continue...", ConsoleColor.Gray);
            Console.ReadKey();
            Console.Clear();
            WriteIntro();
            Console.WriteLine();
            Console.WriteLine();
        }

        #endregion



    }
}
