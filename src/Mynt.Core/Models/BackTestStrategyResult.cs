using System;
using System.Collections.Generic;
using System.Linq;

namespace Mynt.Backtester.Models
{
    public class BackTestStrategyResult
    {
        public string Strategy { get; set; }

        public decimal AverageDuration => Results.Count > 0 ? (decimal)Results.Average(x => x.AverageDuration) : 0.0m;
        public int AmountOfTrades => Results.Count > 0 ? Results.Sum(x => x.AmountOfTrades) : 0;
        public int AmountOfProfitableTrades => Results.Count > 0 ? Results.Sum(x => x.AmountOfProfitableTrades) : 0;
        public decimal TotalProfitPercentage => Results.Count > 0 ? Results.Sum(x => x.TotalProfitPercentage) : 0;
        public decimal TotalProfit => Results.Count > 0 ? Results.Sum(x => x.TotalProfit) : 0;
        public decimal SuccessRate => AmountOfTrades > 0 ? ((decimal)AmountOfProfitableTrades / (decimal)AmountOfTrades) * 100.0m : 0.0m;

        public decimal DataPeriod => Results.Count > 0 ? Results.Max(x => x.DataPeriod) : 0;

        public List<BackTestResult> Results { get; set; }

        public BackTestStrategyResult()
        {
            Results = new List<BackTestResult>();
        }
    }
}
