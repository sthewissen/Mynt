using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class SimpleBearBull : ITradingStrategy
    {
        public string Name => "The Bull & The Bear";

        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var closes = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i >= 2)
                {
                    var current = closes[i];
                    var previous = closes[i - 1];
                    var prior = closes[i - 2];

                    if (current > previous && previous > prior)
                        result.Add(1);
                    else if (current < previous)
                        result.Add(-1);
                    else
                        result.Add(0);
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
