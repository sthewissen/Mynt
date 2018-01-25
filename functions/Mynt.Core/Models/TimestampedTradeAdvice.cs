using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Enums;

namespace Mynt.Core.Models
{
    public class TimestampedTradeAdvice : ITradeAdvice
    {
        private readonly DateTime timestamp;

        private readonly TradeAdvice tradeAdvice;

        public TimestampedTradeAdvice(DateTime timestamp, TradeAdvice tradeAdvice)
        {
            this.tradeAdvice = tradeAdvice;
            this.timestamp = timestamp;
        }

        public DateTime Timestamp => timestamp;

        public TradeAdvice TradeAdvice => tradeAdvice;

        public override string ToString()
        {
            return $"TradeAdvice at {timestamp.ToString()}: {tradeAdvice.ToString()}";
        }
    }
}
