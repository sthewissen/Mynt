using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class SimpleBearBull : ITradingStrategy
    {
        public string Name => "The Bull & The Bear";

        public List<TradeAdvice> Prepare(List<Candle> candles)
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
                    result.Add(0);
                }
            }

            return result;
        }
    }
}
