using System;
using System.Collections.Generic;
using System.Text;

namespace Mynt.BackTester.Models
{
    internal class StrategyResult
    {
        public string Name { get; set; }
        public int TotalTrades { get; set; }
        public int ProfitTrades { get; set; }
        public int NonProfitTrades { get; set; }
        public double TotalProfit { get; set; }
        public double AvgProfit { get; set; }
        public double AvgTime { get; set; }
        public double Grade { get; set; }
    }
}
