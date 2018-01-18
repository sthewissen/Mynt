using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// This is a strategy most suited for 30 minute ticks.
    /// </summary>
    public class SarAwesome : ITradingStrategy
    {
        public string Name => "SAR Awesome";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var sar = candles.Sar();
            var ema5 = candles.Ema(5);
            var ao = candles.AwesomeOscillator();

            var close = candles.Select(x => x.Close).ToList();
            var highs = candles.Select(x => x.High).ToList();
            var lows = candles.Select(x => x.Low).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i >= 2)
                {
                    var currentSar = sar[i];
                    var priorSar = sar[i - 1];
                    var earlierSar = sar[i - 2];
                    var lastHigh = highs[i];
                    var lastLow = lows[i];

                    if ((currentSar > lastHigh) && (priorSar > lastHigh) && (earlierSar > lastHigh) && ao[i] > 0 && ema5[i] < close[i])
                        result.Add(1);
                    else if ((currentSar < lastLow) && (priorSar < lastLow) && (earlierSar < lastLow) && ao[i] < 0 && ema5[i] > close[i])
                        result.Add(-1);
                    else
                        result.Add(0);
                }
                else
                {
                    result.Add(0);
                }
            }

            return result;
        }
    }
}