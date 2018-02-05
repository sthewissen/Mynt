using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class BuyAndHold : BaseStrategy
    {
        public override string Name => "Buy & Hold";
        public override int MinimumAmountOfCandles => 20;
        public override Period IdealPeriod => Period.Hour;

        public override List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice> {new SimpleTradeAdvice(TradeAdvice.Buy)};
            var holdAdvices = new int[candles.Count - 1];

            result.AddRange(holdAdvices.Select(_=> new SimpleTradeAdvice((TradeAdvice)_)));

            return result;
        }

        public override ITradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}
