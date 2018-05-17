using System;
using System.Collections.Generic;

namespace Mynt.Core.Backtester
{
    public class BacktestOptions
    {
        public static string Exchange { get; set; } = "Binance";
        public static decimal StakeAmount { get; set; } = 0.1m;
        public static bool OnlyStartNewTradesWhenSold { get; set; } = true;
        public static bool ConsoleMode { get; set; } = false;
        public static List<string> Coins { get; set; } = new List<string> { };
        public static int CandlePeriod { get; set; } = 60;
        public static bool UpdateCandles { get; set; } = true;
        public static string StartDate { get; set; } = "2018-01-01";
        public static string EndDate { get; set; } = null;
    }
}
