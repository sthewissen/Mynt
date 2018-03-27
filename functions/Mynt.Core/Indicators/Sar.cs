using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;
using TicTacTec.TA.Library;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static List<decimal?> Sar(this List<Candle> source, double accelerationFactor = 0.02, double maximumAccelerationFactor = 0.2)
        {
            int outBegIdx, outNbElement;
            double[] sarValues = new double[source.Count];

            var highs = source.Select(x => Convert.ToDouble(x.High)).ToArray();
            var lows = source.Select(x => Convert.ToDouble(x.Low)).ToArray();

            var sar = TicTacTec.TA.Library.Core.Sar(0, source.Count - 1, highs, lows, 0.02, 0.2, out outBegIdx, out outNbElement, sarValues);

            if (sar == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                // party
                return FixIndicatorOrdering(sarValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate SAR!");
        }
    }
}
