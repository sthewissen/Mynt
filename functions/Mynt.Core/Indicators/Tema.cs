using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
    public static partial class Extensions
    {
        public static List<double?> Tema(this List<Candle> source, int period = 20, CandleVariable type = CandleVariable.Close)
        {
            int outBegIdx, outNbElement;
            double[] temaValues = new double[source.Count];
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

            var tema = TicTacTec.TA.Library.Core.Tema(0, source.Count - 1, valuesToCheck, period, out outBegIdx, out outNbElement, temaValues);

            if (tema == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(temaValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate TEMA!");
        }
    }
}
