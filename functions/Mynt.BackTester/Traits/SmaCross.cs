using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.BackTester.Traits
{
    public class SmaCross : ITrait
    {
        public List<int> Create(List<Candle> candles)
        {
            var sma10 = candles.Sma(10);
            var sma20 = candles.Sma(20);
            List<int> result = new List<int>();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i==0)
                    result.Add(0);
                else if (sma20[i-1] > sma10[i-1] && sma20[i] < sma10[i])
                    result.Add(1);
                else if (sma20[i - 1] < sma10[i - 1] && sma20[i] > sma10[i])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
