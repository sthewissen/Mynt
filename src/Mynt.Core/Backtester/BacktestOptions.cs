using System;
using System.Collections.Generic;
using System.IO;

namespace Mynt.Core.Backtester
{
    public class BacktestOptions
    {
		public Enums.Exchange Exchange { get; set; } = Enums.Exchange.Binance;
        public string DataFolder { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        public decimal StakeAmount { get; set; } = 0.1m;
        public bool OnlyStartNewTradesWhenSold { get; set; } = true;
        public List<string> Coins { get; set; } = new List<string>();
        public int CandlePeriod { get; set; } = 60;
        public bool UpdateCandles { get; set; } = true;
        public string StartDate { get; set; } = "2018-01-01";
        public string EndDate { get; set; } = null;
    }
}
