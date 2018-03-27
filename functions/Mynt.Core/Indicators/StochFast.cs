using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static Stoch StochFast(this List<Candle> source, int fastKPeriod = 5, int fastDPeriod = 3, TicTacTec.TA.Library.Core.MAType fastDmaType = TicTacTec.TA.Library.Core.MAType.Sma)
        {
            int outBegIdx, outNbElement;
            double[] kValues = new double[source.Count];
            double[] dValues = new double[source.Count];

            var highs = source.Select(x => Convert.ToDouble(x.High)).ToArray();
            var lows = source.Select(x => Convert.ToDouble(x.Low)).ToArray();
            var closes = source.Select(x => Convert.ToDouble(x.Close)).ToArray();

            var tema = TicTacTec.TA.Library.Core.StochF(0, source.Count - 1, highs, lows, closes, fastKPeriod, fastDPeriod,
                 fastDmaType, out outBegIdx, out outNbElement, kValues, dValues);

            if (tema == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return new Stoch()
                {
                    D = FixIndicatorOrdering(dValues.ToList(), outBegIdx, outNbElement),
                    K = FixIndicatorOrdering(kValues.ToList(), outBegIdx, outNbElement)
                };
            }

            throw new Exception("Could not calculate STOCH!");
        }
    }
}