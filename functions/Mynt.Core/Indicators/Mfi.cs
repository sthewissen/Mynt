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
        public static List<double?> Mfi(this List<Candle> source, int period = 14)
        {
            int outBegIdx, outNbElement;
            double[] mfiValues = new double[source.Count];

            var mfi = TicTacTec.TA.Library.Core.Mfi(0, source.Count - 1, source.Select(x => x.High).ToArray(),
                 source.Select(x => x.Low).ToArray(), source.Select(x => x.Close).ToArray(),
                 source.Select(x => x.Volume).ToArray(), period, out outBegIdx, out outNbElement, mfiValues);

            if (mfi == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(mfiValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate MFI!");
        }
    }
}
