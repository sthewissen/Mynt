using System;
using System.Collections.Generic;
using System.Linq;

namespace Mynt.Backtester.Models
{
    public class BackTestResult
    {
        public string Market { get; set; }

        public decimal AverageDuration => Trades.Count > 0 ? (decimal)Trades.Average(x => x.Duration) : 0.0m;
        public int AmountOfTrades => Trades.Count;
        public int AmountOfProfitableTrades => Trades.Count > 0 ? Trades.Count(x => x.ProfitPercentage > 0) : 0;
        public decimal TotalProfitPercentage => Trades.Count > 0 ? Trades.Sum(x => x.ProfitPercentage) : 0;
        public decimal TotalProfit => Trades.Count > 0 ? Trades.Sum(x => x.Profit) : 0;
        public decimal SuccessRate => Trades.Count > 0 ? ((decimal)AmountOfProfitableTrades / (decimal)AmountOfTrades) * 100.0m : 0.0m;

        public decimal DataPeriod => Trades.Count > 0 ? (Trades.Max(x => x.EndDate) - Trades.Min(x => x.StartDate)).Days : 0;

        public List<BackTestTradeResult> Trades { get; set; }

        public BackTestResult()
        {
            Trades = new List<BackTestTradeResult>();
        }
    }
}
