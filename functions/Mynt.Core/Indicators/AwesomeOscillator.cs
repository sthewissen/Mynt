using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static List<decimal?> AwesomeOscillator(this List<Candle> source, bool returnRaw = false)
        {
            // Calculate our Moving Averages
            var smaFast = source.Select(x => (x.High + x.Low) / 2).ToList().Sma(5);
            var smaSlow = source.Select(x => (x.High + x.Low) / 2).ToList().Sma(34);

            var result = new List<decimal?>();

            for (var i = 0; i < smaFast.Count; i++)
            {
                if (returnRaw)
                {
                    if (!smaFast[i].HasValue || !smaSlow[i].HasValue)
                        result.Add(null);
                    else
                        result.Add(smaFast[i].Value - smaSlow[i].Value);
                }
                else
                {
                    // The last and second to last values interest us, because we're looking for a cross of these lines.
                    // If it's not the first item, we can check the previous.
                    if (i > 0)
                    {
                        var smaFastLast = smaFast[i];
                        var smaSlowLast = smaSlow[i];
                        var smaFastSecondLast = smaFast[i - 1];
                        var smaSlowSecondLast = smaSlow[i - 1];

                        var aoSecondLast = smaFastSecondLast - smaSlowSecondLast;
                        var aoLast = smaFastLast - smaSlowLast;

                        if (aoSecondLast <= 0 && aoLast > 0)
                            result.Add(100);
                        else if (aoSecondLast >= 0 && aoLast < 0)
                            result.Add(-100);
                        else
                            result.Add(0);
                    }
                    else
                    {
                        result.Add(0);
                    }
                }
            }

            return result;
        }
    }
}
