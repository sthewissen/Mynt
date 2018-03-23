using System;
using System.Collections.Generic;
using Mynt.Core.Api;
using Mynt.Core.Binance;
using Mynt.Core.Bittrex;
using Mynt.Core.Interfaces;
using Mynt.Core.Strategies;
using Mynt.DataAccess.FileBasedStorage;

namespace Mynt.BackTester.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string coinsToBuyCsv = System.Configuration.ConfigurationManager.AppSettings["CoinsToBuy"];

            var settings = new Core.Constants();
            settings.IsDryRunning = true;

            IExchangeApi api;
            // Use Bittrex if API Key provided
            // or Binance if it's not (one of both should be set)
            if (!String.IsNullOrEmpty(settings.BittrexApiKey))
            {
                api = new BittrexApi(settings, true);
            }
            else
            {
                api = new BinanceApi(settings);
            }

            var backTester = new BackTester(GetTradingStrategies(), api, new CsvDataStorage("DataStorage"), coinsToBuyCsv);
            try
            {
                backTester.WriteIntro();
                System.Console.WriteLine();
                System.Console.WriteLine();
                backTester.PresentMenuToUser();
            }
            catch (Exception ex)
            {
                backTester.WriteColoredLine($"\t{ex.Message}", ConsoleColor.Red);
                System.Console.ReadLine();
            }
        }

        private static List<ITradingStrategy> GetTradingStrategies()
        {
            return new List<ITradingStrategy>()
            {
                // The strategies we want to backtest.
                new BigThree(),
                new BollingerAwe(),
                new BuyAndHold(),
                new SmaCrossover(),
                new TheScalper(),
                new Wvf(),
                new WvfExtended()
            };
        }
    }
}
