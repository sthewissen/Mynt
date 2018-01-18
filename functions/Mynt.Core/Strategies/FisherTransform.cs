using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class FisherTransform : ITradingStrategy
    {
        public string Name => "Fisher Transform";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();
            var fishers = candles.Fisher(10);
            var ao = candles.AwesomeOscillator();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (fishers[i] < 0 && fishers[i - 1] > 0 && ao[i] < 0)
                    result.Add(1);
                else if (fishers[i] == 1)
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
