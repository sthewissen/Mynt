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
        public static MamaItem Mama(this List<Candle> source, double fastLimit = 0, double slowLimit = 0)
        {
            int outBegIdx, outNbElement;
            double[] mamaValues = new double[source.Count];
            double[] famaValues = new double[source.Count];

            var mfi = TicTacTec.TA.Library.Core.Mama(0, source.Count - 1, source.Select(x => x.Close).ToArray(),
                fastLimit, slowLimit, out outBegIdx, out outNbElement, mamaValues, famaValues);

            if (mfi == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return new MamaItem
                {
                    Mama = FixIndicatorOrdering(mamaValues.ToList(), outBegIdx, outNbElement),
                    Fama = FixIndicatorOrdering(famaValues.ToList(), outBegIdx, outNbElement)
                };
            }

            throw new Exception("Could not calculate MAMA!");
        }
    }

    public class MamaItem
    {
        public List<double?> Mama { get; set; }
        public List<double?> Fama { get; set; }
    }
}
