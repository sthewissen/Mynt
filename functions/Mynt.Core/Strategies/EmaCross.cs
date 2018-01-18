using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class EmaCross : ITradingStrategy
    {
        public string Name => "EMA Cross";

        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var ema12 = candles.Ema(12);
            var ema26 = candles.Ema(26);

            for (int i = 0; i < candles.Count; i++)
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
