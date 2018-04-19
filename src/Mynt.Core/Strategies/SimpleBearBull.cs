using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class SimpleBearBull : BaseStrategy
    {
        public override string Name => "The Bull & The Bear";
        public override int MinimumAmountOfCandles => 5;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var closes = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i >= 2)
                {
                    var current = closes[i];
                    var previous = closes[i - 1];
                    var prior = closes[i - 2];

                    if (current > previous && previous > prior)
                        result.Add(TradeAdvice.Buy);
                    else if (current < previous)
                        result.Add(TradeAdvice.Sell);
                    else
                        result.Add(TradeAdvice.Hold);
                }
                else
                {
                    result.Add(TradeAdvice.Hold);
                }
            }

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
