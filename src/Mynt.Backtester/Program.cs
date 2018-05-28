using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Mynt.Core.Backtester;
using Mynt.Core.Interfaces;
using Mynt.Core.Utility;
using Mynt.Data.LiteDB;
//using Mynt.Data.MongoDB;

namespace Mynt.Backtester
{
	class Program
    {
        public static IConfiguration Configuration { get; set; }
        public static BacktestOptions BacktestOptions { get; set; }
        public static IDataStoreBacktest DataStore { get; set; }
        public static int WriteMenuCount;

        static void Main(string[] args)
        {
            try
            {
                Init();

                WriteIntro();
                Console.WriteLine();
                Console.WriteLine();

                if (!DataRefresher.CheckForCandleData(BacktestOptions, DataStore).Result)
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
            BacktestOptions = Configuration.Get<BacktestOptions>();


            LiteDBOptions backtestDatabaseOptions = new LiteDBOptions();
            DataStore = new LiteDBDataStoreBacktest(backtestDatabaseOptions);

            //MongoDBOptions backtestDatabaseOptions = new MongoDBOptions();
            //DataStore = new MongoDBDataStoreBacktest(backtestDatabaseOptions);
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
                            BacktestFunctions.BackTestConsole(strats[index - 1], BacktestOptions, DataStore);
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
                            BacktestFunctions.BackTestShowTradesConsole(strats[indexStrategy - 1], BacktestOptions, DataStore);
                        }
                        else
                        {
                            ConsoleUtility.WriteColoredLine("\tInvalid strategy choice.", ConsoleColor.Red);
                            PresentMenuToUser();
                        }

                        continue;

                    case "3":
                        Console.WriteLine("\tBacktesting all strategies takes some time and uses historic data. Starting...");
                        BacktestFunctions.BackTestAllConsole(BacktestOptions, DataStore);
                        continue;

                    case "4":
                        Console.WriteLine("\tRefreshing...");
                        DataRefresher.RefreshCandleData((x) => ConsoleUtility.WriteColoredLine(x, ConsoleColor.Green), BacktestOptions, DataStore).Wait();
                        ActionCompleted();
                        WriteMenuCount = 0;
                        continue;

                    case "5":
                    default:
                        Environment.Exit(1);
                        break;
                }
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
            if (WriteMenuCount == 0)
            {
                DataRefresher.GetCacheAgeConsole(BacktestOptions, DataStore);
            }
            WriteMenuCount = WriteMenuCount + 1;

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
