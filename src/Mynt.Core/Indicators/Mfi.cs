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
        public static List<decimal?> Mfi(this List<Candle> source, int period = 14)
        {
            int outBegIdx, outNbElement;
            double[] mfiValues = new double[source.Count];

            var highs = source.Select(x => Convert.ToDouble(x.High)).ToArray();
            var lows = source.Select(x => Convert.ToDouble(x.Low)).ToArray();
            var closes = source.Select(x => Convert.ToDouble(x.Close)).ToArray();
            var volumes = source.Select(x => Convert.ToDouble(x.Volume)).ToArray();

            var mfi = TicTacTec.TA.Library.Core.Mfi(0, source.Count - 1, highs, lows, closes, volumes, period, out outBegIdx, out outNbElement, mfiValues);

            if (mfi == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(mfiValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate MFI!");
        }
    }
}
