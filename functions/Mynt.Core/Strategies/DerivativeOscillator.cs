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
    public class DerivativeOscillator : ITradingStrategy
    {
        public string Name => "Derivative Oscillator";
        public List<Candle> Candles { get; set; }
        public DerivativeOscillator()
        {
            this.Candles = new List<Candle>();
        }
        public List<int> Prepare()
        {
            var result = new List<int>();
            var derivativeOsc = Candles.DerivativeOscillator();

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (derivativeOsc[i - 1] < 0 && derivativeOsc[i] > 0)
                    result.Add(1);
                else if (derivativeOsc[i] >= 0 && derivativeOsc[i] <= derivativeOsc[i-1])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
