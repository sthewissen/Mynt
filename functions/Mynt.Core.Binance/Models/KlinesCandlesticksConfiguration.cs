using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Binance.Models
{
    public class KlinesCandlesticksConfiguration
    {
        public string Symbol { get; set; }

        public KlineInterval Interval { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int Limit { get; set; }
    }
}
