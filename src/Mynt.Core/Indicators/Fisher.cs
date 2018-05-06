using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static List<int?> Fisher(this List<Candle> source, int period = 10)
        {
            var nValues1 = new List<decimal>();
            var fishers = new List<decimal>();
            var result = new List<int?>();
            var highLowAverages = source.Select(x => (x.High + x.Low) / 2).ToList();

            for (int i = 0; i < source.Count; i++)
            {
                if (i < 2)
                {
                    result.Add(null);
                    nValues1.Add(0);
                    fishers.Add(0);
                }
                else
                {
                    var maxH = 0.0m;
                    var minH = 0.0m;

                    if (i < period - 1)
                    {
                        maxH = highLowAverages.Take(i + 1).Max();
                        minH = highLowAverages.Take(i + 1).Min();
                    }
                    else
                    {
                        maxH = highLowAverages.Skip(i + 1 - period).Take(period).Max();
                        minH = highLowAverages.Skip(i + 1 - period).Take(period).Min();
                    }

                    var nValue1 = 0.33m * 2 * ((highLowAverages[i] - minH) / (maxH - minH) - 0.5m) +
                                  0.67m * nValues1[i - 1];
                    nValues1.Add(nValue1);

                    var nValue2 = nValue1 > 0.99m ? .999m : (nValue1 < -.99m ? -.999m : nValue1);

                    var nFish = 0.5m * (decimal)Math.Log((double)((1 + nValue2) / (1 - nValue2))) + 0.5m * fishers[i - 1];
                    fishers.Add(nFish);

                    if (fishers[i] > fishers[i - 1])
                        result.Add(1);
                    else if (fishers[i] < fishers[i - 1])
                        result.Add(-1);
                    else
                        result.Add(0);
                }
            }

            return result;
        }
    }
}
