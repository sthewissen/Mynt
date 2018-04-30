using System;
using System.Collections.Generic;

namespace Mynt.Backtester
{
    public class BacktestOptions
    {
        public decimal StakeAmount { get; set; } = 0.1m;
        public bool OnlyStartNewTradesWhenSold { get; set; } = true;
        public List<string> Coins { get; set; } = new List<string> { };
    }
}
