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
        public static Stoch StochRsi(this List<Candle> source, int optInTimePeriod = 14, CandleVariable type = CandleVariable.Close,
                                     int fastKPeriod = 5, int fastDPeriod = 3,
            TicTacTec.TA.Library.Core.MAType fastDmaType = TicTacTec.TA.Library.Core.MAType.Sma)
        {
            int outBegIdx, outNbElement;
            double[] kValues = new double[source.Count];
            double[] dValues = new double[source.Count];
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

            var tema = TicTacTec.TA.Library.Core.StochRsi(0, source.Count - 1, valuesToCheck, optInTimePeriod, fastKPeriod, fastDPeriod,
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
