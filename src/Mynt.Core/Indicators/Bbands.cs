using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static Bband Bbands(this List<Candle> source, int period = 5, double devUp = 2, double devDown = 2, TicTacTec.TA.Library.Core.MAType type = TicTacTec.TA.Library.Core.MAType.Sma)
        {
            int outBegIdx, outNbElement;
            double[] upperValues = new double[source.Count];
            double[] middleValues = new double[source.Count];
            double[] lowerValues = new double[source.Count];
            var closes = source.Select(x => Convert.ToDouble(x.Close)).ToArray();

            var bbands = TicTacTec.TA.Library.Core.Bbands(0, source.Count - 1, closes,
                period, devUp, devDown, type, out outBegIdx, out outNbElement, upperValues, middleValues, lowerValues);

            if (bbands == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return new Bband()
                {
                    UpperBand = FixIndicatorOrdering(upperValues.ToList(), outBegIdx, outNbElement),
                    MiddleBand = FixIndicatorOrdering(middleValues.ToList(), outBegIdx, outNbElement),
                    LowerBand = FixIndicatorOrdering(lowerValues.ToList(), outBegIdx, outNbElement)
                };
            }

            throw new Exception("Could not calculate Bbands!");
        }
    }

    public class Bband
    {
        public List<decimal?> UpperBand { get; set; }
        public List<decimal?> MiddleBand { get; set; }
        public List<decimal?> LowerBand { get; set; }
    }
}
