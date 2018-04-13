using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Interfaces;
using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using System;
using System.Collections.Generic;

namespace Mynt.BackTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var coinsToBuy = new List<string> { "NEOBTC", "ETHBTC", "LTCBTC", "XRPBTC", "XLMBTC", "BCCBTC" };
            
            var backTester = new BackTester(
                GetTradingStrategies(), 
                new BaseExchange(new ExchangeOptions() { Exchange = Exchange.Binance}),
                new TradeOptions(),
                coinsToBuy
            );

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
                new BuyAndHold(),
                new GoldenCross(),
                new TheScalper(),
            };
        }
    }
}
