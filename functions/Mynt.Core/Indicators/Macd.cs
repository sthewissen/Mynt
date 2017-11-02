using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static MacdItem Macd(this List<Candle> source, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            int outBegIdx, outNbElement;
            double[] macdValues = new double[source.Count];
            double[] signalValues = new double[source.Count];
            double[] histValues = new double[source.Count];

            var macd = TicTacTec.TA.Library.Core.Macd(0, source.Count - 1, source.Select(x => x.Close).ToArray(),
                fastPeriod, slowPeriod, signalPeriod, out outBegIdx, out outNbElement, macdValues, signalValues, histValues);

            if (macd == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return new MacdItem()
                {
                    Macd = FixIndicatorOrdering(macdValues.ToList(), outBegIdx, outNbElement),
                    Signal = FixIndicatorOrdering(signalValues.ToList(), outBegIdx, outNbElement),
                    Hist = FixIndicatorOrdering(histValues.ToList(), outBegIdx, outNbElement)
                };
            }

            throw new Exception("Could not calculate MACD!");
        }
    }

    public class MacdItem
    {
        public List<double?> Macd { get; set; }
        public List<double?> Signal { get; set; }
        public List<double?> Hist { get; set; }
    }
}
