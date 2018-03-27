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
        public static List<decimal?> Mom(this List<Candle> source, int period = 10)
        {
            int outBegIdx, outNbElement;
            double[] momValues = new double[source.Count];

            var closes = source.Select(x => Convert.ToDouble(x.Close)).ToArray();

            var mom = TicTacTec.TA.Library.Core.Mom(0, source.Count - 1, closes, period, out outBegIdx, out outNbElement, momValues);

            if (mom == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(momValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate MOM!");
        }
    }
}
