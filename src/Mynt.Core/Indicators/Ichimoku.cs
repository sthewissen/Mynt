using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static IchimokuItem Ichimoku(this List<Candle> source, int conversionLinePeriod = 20, int baseLinePeriod = 60, int laggingSpanPeriods = 120, int displacement = 30)
        {
            try
            {
                var highs = source.Select(x => x.High).ToList();
                var lows = source.Select(x => x.Low).ToList();
                var closes = source.Select(x => x.Close).ToList();

                var ichi = new IchimokuItem();

                ichi.TenkanSen = Donchian(source, conversionLinePeriod, highs, lows);
                ichi.KijunSen = Donchian(source, baseLinePeriod, highs, lows);
                ichi.SenkouSpanB = Donchian(source, laggingSpanPeriods, highs, lows);

                // SenkouSpanA is calculated...
                ichi.SenkouSpanA = new List<decimal?>();

                for (int i = 0; i < ichi.TenkanSen.Count; i++)
                {
                    if (ichi.TenkanSen[i].HasValue && ichi.KijunSen[i].HasValue)
                    {
                        ichi.SenkouSpanA.Add((ichi.TenkanSen[i].Value + ichi.KijunSen[i].Value) / 2);
                    }
                    else
                    {
                        ichi.SenkouSpanA.Add(null);
                    }
                }

                // Add the displacement for the cloud
                for (int i = 0; i < displacement; i++)
                {
                    ichi.SenkouSpanA.Insert(0, null);
                    ichi.SenkouSpanB.Insert(0, null);
                }

                // Add the ChikouSpan
                ichi.ChikouSpan = new List<decimal?>();

                // Add the displacement for the lagging span
                var displacedCloses = closes.Skip(displacement).ToList();

                for (int i = 0; i < closes.Count; i++)
                {
                    if (i < closes.Count - displacement)
                        ichi.ChikouSpan.Add(displacedCloses[i]);
                    else
                        ichi.ChikouSpan.Add(null);
                }

                return ichi;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not calculate ichimoku cloud!");
            }
        }

        private static List<decimal?> Donchian(List<Candle> source, int period, List<decimal> highs, List<decimal> lows)
        {
            // Calculate the Tenkan-sen
            var result = new List<decimal?>();

            for (var i = 0; i < source.Count; i++)
            {
                if (i < period)
                {
                    // Get the highest high & lowest low of the last X items (X = conversionLinePeriod)
                    var highestHigh = highs.GetRange(0, i + 1).Max();
                    var lowestLow = lows.GetRange(0, i + 1).Min();

                    result.Add((highestHigh + lowestLow) / 2);
                }
                else
                {
                    // Get the highest high & lowest low of the last X items (X = conversionLinePeriod)
                    var highestHigh = highs.GetRange(i - period, period).Max();
                    var lowestLow = lows.GetRange(i - period, period).Min();

                    result.Add((highestHigh + lowestLow) / 2);
                }
            }

            return result;
        }
    }

    public class IchimokuItem
    {
        public List<decimal?> TenkanSen { get; set; }
        public List<decimal?> KijunSen { get; set; }
        public List<decimal?> SenkouSpanA { get; set; }
        public List<decimal?> SenkouSpanB { get; set; }
        public List<decimal?> ChikouSpan { get; set; }
    }
}

