using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// Technically this is a 1d, but can be used more for e.g. 1 hour ticks.
    /// </summary>
    public class BreakoutMa : ITradingStrategy
    {
        public string Name => "Breakout MA";

        public List<Models.Candle> Candles { get; set; }

        public BreakoutMa()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var sma20 = Candles.Sma(20, CandleVariable.Low);
            var ema34 = Candles.Ema(34);
            var adx = Candles.Adx(13);

            for (int i = 0; i < Candles.Count; i++)
            {
                if (ema34[i] > sma20[i] && adx[i] > 25)
                    result.Add(1);
                else if (ema34[i] < sma20[i] && adx[i] > 25)
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
