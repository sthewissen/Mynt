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
    public class FaMaMaMa : ITradingStrategy
    {
        public string Name => "FAMAMAMA";

        public List<Candle> Candles { get; set; }

        public FaMaMaMa()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var mama = Candles.Mama(0.5, 0.05);
            var fama = Candles.Mama(0.25, 0.025);

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (fama.Mama[i] > mama.Mama[i] && fama.Mama[i - 1] < mama.Mama[i])
                    result.Add(1);
                else if (fama.Mama[i] < mama.Mama[i] && fama.Mama[i - 1] > mama.Mama[i])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}