using Mynt.Core.Enums;
using Mynt.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mynt.Core.Strategies
{
    public class BuyAndHold : BaseStrategy
    {
        public override string Name => "Buy & Hold";
        public override int MinimumAmountOfCandles => 20;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice> { TradeAdvice.Buy };
            var holdAdvices = new TradeAdvice[candles.Count - 1];

            result.AddRange(holdAdvices);

            return result;
        }

        public override Candle GetSignalCandle(List<Candle> candles)
        {
            return candles.Last();
        }

        public override TradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}
