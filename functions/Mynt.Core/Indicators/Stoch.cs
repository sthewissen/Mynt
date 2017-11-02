using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static Stoch Stoch(this List<Candle> source, int fastKPeriod = 5, int slowKPeriod = 3, 
            TicTacTec.TA.Library.Core.MAType slowKmaType = TicTacTec.TA.Library.Core.MAType.Sma, 
            int slowDPeriod = 3, TicTacTec.TA.Library.Core.MAType slowDmaType = TicTacTec.TA.Library.Core.MAType.Sma)
        {
            int outBegIdx, outNbElement;
            double[] kValues = new double[source.Count];
            double[] dValues = new double[source.Count];

            var tema = TicTacTec.TA.Library.Core.Stoch(0, source.Count - 1, source.Select(x => x.High).ToArray(),
                 source.Select(x => x.Low).ToArray(), source.Select(x => x.Close).ToArray(), fastKPeriod, slowKPeriod,
                 slowKmaType, slowDPeriod, slowDmaType, out outBegIdx, out outNbElement, kValues, dValues);

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

    public class Stoch
    {
        public List<double?> K { get; set; }
        public List<double?> D { get; set; }
    }
}



