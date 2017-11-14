using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.BackTester.Traits
{
    public class Rsi : ITrait
    {
        public List<int> Create(List<Candle> candles)
        {
            var rsi = candles.Rsi();
            List<int> result = new List<int>();

            foreach (var value in rsi)
            {
                if (value < 30)
                    result.Add(1);
                else if (value > 70)
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
