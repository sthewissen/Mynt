using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.BackTester.Traits
{
    public class EmaCross : ITrait
    {
        public List<int> Create(List<Candle> candles)
        {
            var ema10 = candles.Ema(10);
            var ema20 = candles.Ema(20);
            List<int> result = new List<int>();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i==0)
                    result.Add(0);
                else if (ema20[i-1] > ema10[i-1] && ema20[i] < ema10[i])
                    result.Add(1);
                else if (ema20[i - 1] < ema10[i - 1] && ema20[i] > ema10[i])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
