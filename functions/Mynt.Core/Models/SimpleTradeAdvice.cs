using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Enums;

namespace Mynt.Core.Models
{
    public class SimpleTradeAdvice : ITradeAdvice
    {
        private readonly TradeAdvice tradeAdvice;

        private readonly double price;

        public SimpleTradeAdvice(TradeAdvice tradeAdvice, double price = double.NaN)
        {
            this.tradeAdvice = tradeAdvice;
            this.price = price;
        }

        public TradeAdvice TradeAdvice => tradeAdvice;

        public double Price => price;

        public override string ToString()
        {
            return $"TradeAdvice: {tradeAdvice.ToString()}";
        }
    }
}
