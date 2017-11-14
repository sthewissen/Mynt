using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.BackTester.Traits
{
    public class Ao : ITrait
    {
        public List<int> Create(List<Candle> candles)
        {
            var awesomeOscillator = candles.AwesomeOscillator();
            List<int> result = new List<int>();

            foreach (var value in awesomeOscillator)
            {
                if (value > 0)
                    result.Add(1);
                if (value < 0)
                    result.Add(1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
