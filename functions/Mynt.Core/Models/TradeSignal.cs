using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;

namespace Mynt.Core.Models
{
    public class TradeSignal
    {
        public string Pair { get; set; }
        public decimal Price { get; set; }
        public TradeAdvice TradeAdvice { get; set; }
        public Candle SignalCandle { get; set; }
    }
}
