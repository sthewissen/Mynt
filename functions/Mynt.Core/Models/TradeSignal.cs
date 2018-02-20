using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Models
{
    public class TradeSignal
    {
        public string Pair { get; set; }
        public ITradeAdvice TradeAdvice { get; set; }
        public Candle SignalCandle { get; set; }
    }
}
