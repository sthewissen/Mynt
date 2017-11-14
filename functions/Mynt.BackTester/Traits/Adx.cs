using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.BackTester.Traits
{
    public class Adx : ITrait
    {
        public List<int> Create(List<Candle> candles)
        {
            var adx = candles.Adx();
            List<int> result = new List<int>();

            foreach (var value in adx)
            {
                if (value > 50)
                    result.Add(1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
