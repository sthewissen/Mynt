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
        public List<Candle> Candles { get; set; }
        public FisherTransform()
        {
            this.Candles = new List<Candle>();
        }
        public List<int> Prepare()
        {

            var result = new List<int>();
            var fishers = Candles.Fisher(10);
            var ao = Candles.AwesomeOscillator();

            for (int i = 0; i < Candles.Count; i++)
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
