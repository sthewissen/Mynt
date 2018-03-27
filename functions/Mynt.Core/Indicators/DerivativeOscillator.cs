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
        public static List<decimal?> DerivativeOscillator(this List<Candle> source)
        {
            var rsi = source.Rsi();
            var ema1 = rsi.Where(x => x.HasValue).Select(x => x.Value).ToList().Ema(5);
            var ema2 = ema1.Where(x => x.HasValue).Select(x => x.Value).ToList().Ema(3);
            var sma = ema2.Where(x => x.HasValue).Select(x => x.Value).ToList().Sma(9);

            for (int i = sma.Count; i < source.Count; i++)
                sma.Insert(0, null);

            for (int i = ema2.Count; i < source.Count; i++)
                ema2.Insert(0, null);

            var derivativeOsc = new List<decimal?>();

            for (int i = 0; i < sma.Count; i++)
            {
                if (sma[i] == null || ema2[i] == null)
                    derivativeOsc.Add(null);
                else
                    derivativeOsc.Add(ema2[i] - sma[i]);
            }

            return derivativeOsc;
        }
    }
}



