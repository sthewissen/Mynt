using System;
using System.Collections.Generic;
using System.Text;

namespace Mynt.Core.Models
{
    public class MarketData
    {
        public string Name { get; set; }
        public List<Candle> Candles { get; set; }
        public List<int> Trend { get; set; }
    }
}
