using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class BuyAndHold : ITradingStrategy
    {
        public string Name => "Buy & Hold";

        public List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();
            result.Add(TradeAdvice.Buy);
            var holdAdvices = new int[candles.Count - 1];
            result.AddRange(holdAdvices.Select(_=>(TradeAdvice)_));
            return result;
        }
    }
}
