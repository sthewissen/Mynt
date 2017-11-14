using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.BackTester.Traits
{
    public class Mfi : ITrait
    {
        public List<int> Create(List<Candle> candles)
        {
            var mfi = candles.Mfi();
            List<int> result = new List<int>();

            foreach (var value in mfi)
            {
                if (value < 20)
                    result.Add(1);
                else if (value > 80)
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
