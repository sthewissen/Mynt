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

        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice> {new SimpleTradeAdvice(TradeAdvice.Buy)};
            var holdAdvices = new int[candles.Count - 1];

            result.AddRange(holdAdvices.Select(_=> new SimpleTradeAdvice((TradeAdvice)_)));

            return result;
        }

        public ITradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}
