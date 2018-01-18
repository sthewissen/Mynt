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
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var sma20 = candles.Sma(20, CandleVariable.Low);
            var ema34 = candles.Ema(34);
            var adx = candles.Adx(13);

            for (int i = 0; i < candles.Count; i++)
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
