using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        /// <summary>
        /// A very simple check if the last two candles were bullish or bearish.
        /// </summary>
        public static List<int> BearBull(this List<Candle> source)
        {
            var closes = source.Select(x => Convert.ToDecimal(x.Close)).ToList();
            var result = new List<int>();

            for (int i = 0; i < closes.Count; i++)
            {
                if (i < 2)
                    result.Add(0);
                else
                {
                    var current = closes[i];
                    var previous = closes[i - 1];
                    var prior = closes[i - 2];

                    if (current > previous && previous > prior)
                        result.Add(1); // last two candles were bullish
                    else if (current < previous && previous < prior)
                        result.Add(-1); // last two candles were bearish
                    else
                        result.Add(0);
                }
            }

            return result;
        }
    }
}
