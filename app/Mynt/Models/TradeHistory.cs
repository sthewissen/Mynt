using System;
using System.Collections.Generic;

namespace Mynt.Models
{
    public class TradeHistory
    {
        public List<Trade> Trades { get; set; }
        public double TotalProfit { get; set; }
        public double TodaysProfit { get; set; }
        public bool HasTotalProfit { get { return TotalProfit > 0; } }
        public bool HasTodayProfit { get { return TodaysProfit > 0; } }

        public double OverallBalance { get; set; }
    }
}
