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

        private readonly double price;

        public TimestampedTradeAdvice(DateTime timestamp, TradeAdvice tradeAdvice, double price = double.NaN)
        {
            this.timestamp = timestamp;
            this.tradeAdvice = tradeAdvice;
            this.price = price;
        }

        public DateTime Timestamp => timestamp;

        public TradeAdvice TradeAdvice => tradeAdvice;

        public double Price => price;
        
        public override string ToString()
        {
            return $"TradeAdvice at {timestamp.ToString()}: {tradeAdvice.ToString()}";
        }
    }
}
