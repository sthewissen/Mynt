using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class EmaCross : ITradingStrategy
    {
        public string Name => "EMA Cross";
        public List<Candle> Candles { get; set; }

        public EmaCross()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var ema12 = Candles.Ema(12);
            var ema26 = Candles.Ema(26);

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (ema12[i] < ema26[i] && ema12[i - 1] > ema26[i])
                    result.Add(1);
                else if (ema12[i] > ema26[i] && ema12[i - 1] < ema26[i])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
