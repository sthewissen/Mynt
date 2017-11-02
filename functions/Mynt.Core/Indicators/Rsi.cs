using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static List<double?> Rsi(this List<Candle> source, int period = 14)
        {
            int outBegIdx, outNbElement;
            double[] rsiValues = new double[source.Count];

            var ema = TicTacTec.TA.Library.Core.Rsi(0, source.Count - 1, source.Select(x => x.Close).ToArray(), period, out outBegIdx, out outNbElement, rsiValues);

            if (ema == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(rsiValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate RSI!");
        }
    }
}
