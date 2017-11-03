using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static List<double?> Sma(this List<Candle> source, int period = 30, CandleVariable type = CandleVariable.Close)
        {
            int outBegIdx, outNbElement;
            double[] smaValues = new double[source.Count];
            List<double?> outValues = new List<double?>();
            double[] valuesToCheck;

            switch (type)
            {
                case CandleVariable.Open:
                    valuesToCheck = source.Select(x => x.Open).ToArray();
                    break;
                case CandleVariable.Low:
                    valuesToCheck = source.Select(x => x.Low).ToArray();
                    break;
                case CandleVariable.High:
                    valuesToCheck = source.Select(x => x.High).ToArray();
                    break;
                default:
                    valuesToCheck = source.Select(x => x.Close).ToArray();
                    break;
            }

            var sma = TicTacTec.TA.Library.Core.Sma(0, source.Count - 1, valuesToCheck, period, out outBegIdx, out outNbElement, smaValues);

            if (sma == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(smaValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate SMA!");
        }

        public static List<double?> Sma(this List<double> source, int period = 30)
        {
            int outBegIdx, outNbElement;
            double[] smaValues = new double[source.Count];
            List<double?> outValues = new List<double?>();

            var sma = TicTacTec.TA.Library.Core.Sma(0, source.Count - 1, source.ToArray(), period, out outBegIdx, out outNbElement, smaValues);

            if (sma == TicTacTec.TA.Library.Core.RetCode.Success)
            {
               return FixIndicatorOrdering(smaValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate SMA!");
        }
    }
}
