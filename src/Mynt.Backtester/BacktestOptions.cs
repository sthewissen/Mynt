using System;
using System.Collections.Generic;

namespace Mynt.Backtester
{
    public class BacktestOptions
    {
        public decimal StakeAmount { get; set; }
        public List<string> Coins { get; set; }
    }
}
