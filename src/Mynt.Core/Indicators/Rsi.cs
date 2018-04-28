using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static List<decimal?> Rsi(this List<Candle> source, int period = 14)
        {
            int outBegIdx, outNbElement;
            double[] rsiValues = new double[source.Count];

            var closes = source.Select(x => Convert.ToDouble(x.Close)).ToArray();

            var ema = TicTacTec.TA.Library.Core.Rsi(0, source.Count - 1, closes, period, out outBegIdx, out outNbElement, rsiValues);

            if (ema == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(rsiValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate RSI!");
        }

        public static List<decimal?> Rsi(this List<decimal> source, int period = 14)
        {
            int outBegIdx, outNbElement;
            double[] rsiValues = new double[source.Count];

            var sourceFix = source.Select(x => Convert.ToDouble(x)).ToArray();

            var ema = TicTacTec.TA.Library.Core.Rsi(0, source.Count - 1, sourceFix, period, out outBegIdx, out outNbElement, rsiValues);

            if (ema == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(rsiValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate RSI!");
        }
    }
}
