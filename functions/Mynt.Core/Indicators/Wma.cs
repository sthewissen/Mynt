using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;
using Mynt.Core.Enums;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static List<decimal?> Wma(this List<Candle> source, int period = 30, CandleVariable type = CandleVariable.Close)
        {
            int outBegIdx, outNbElement;
            double[] wmaValues = new double[source.Count];
            List<double?> outValues = new List<double?>();
            double[] valuesToCheck;


            switch (type)
            {
                case CandleVariable.Open:
                    valuesToCheck = source.Select(x => Convert.ToDouble(x.Open)).ToArray();
                    break;
                case CandleVariable.Low:
                    valuesToCheck = source.Select(x => Convert.ToDouble(x.Low)).ToArray();
                    break;
                case CandleVariable.High:
                    valuesToCheck = source.Select(x => Convert.ToDouble(x.High)).ToArray();
                    break;
                default:
                    valuesToCheck = source.Select(x => Convert.ToDouble(x.Close)).ToArray();
                    break;
            }

            var sma = TicTacTec.TA.Library.Core.Wma(0, source.Count - 1, valuesToCheck, period, out outBegIdx, out outNbElement, wmaValues);

            if (sma == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(wmaValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate WMA!");
        }

        public static List<decimal?> Wma(this List<decimal> source, int period = 30)
        {
            int outBegIdx, outNbElement;
            double[] wmaValues = new double[source.Count];
            List<double?> outValues = new List<double?>();

            var sourceFix = source.Select(x => Convert.ToDouble(x)).ToArray();

            var sma = TicTacTec.TA.Library.Core.Wma(0, source.Count - 1, sourceFix, period, out outBegIdx, out outNbElement, wmaValues);

            if (sma == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(wmaValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate WMA!");
        }
    }
}
