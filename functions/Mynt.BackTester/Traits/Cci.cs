using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.BackTester.Traits
{
    public class Cci : ITrait
    {
        public List<int> Create(List<Candle> candles)
        {
            var cci = candles.Cci();
            List<int> result = new List<int>();

            foreach (var value in cci)
            {
                if (value < -100)
                    result.Add(1);
                else if (value > 100)
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
