using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Enums;

namespace Mynt.Core.Models
{
    public class SimpleTradeAdvice :ITradeAdvice
    {
        private readonly TradeAdvice tradeAdvice;

        public SimpleTradeAdvice(TradeAdvice tradeAdvice)
        {
            this.tradeAdvice = tradeAdvice;
        }

        public TradeAdvice TradeAdvice => tradeAdvice;

        public override string ToString()
        {
            return $"TradeAdvice: {tradeAdvice.ToString()}";
        }
    }
}
