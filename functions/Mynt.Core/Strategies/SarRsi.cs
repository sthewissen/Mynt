using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// This is a strategy most suited for 1 minute ticks.
    /// </summary>
    public class SarRsi : ITradingStrategy
    {
        public string Name => "SAR RSI";

        public List<Candle> Candles { get; set; }

        public SarRsi()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var sar = Candles.Sar();
            var rsi = Candles.Rsi();

            var highs = Candles.Select(x => x.High).ToList();
            var lows = Candles.Select(x => x.Low).ToList();
            var closes = Candles.Select(x => x.Close).ToList();
            var opens = Candles.Select(x => x.Open).ToList();

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i > 2)
                {
                    var currentSar = sar[i];
                    var priorSar = sar[i - 1];
                    var lastHigh = highs[i];
                    var lastLow = lows[i];
                    var lastOpen = opens[i];
                    var lastClose = closes[i];
                    var priorHigh = highs[i - 1];
                    var priorLow = lows[i - 1];
                    var priorOpen = opens[i - 1];
                    var priorClose = closes[i - 1];
                    var prevOpen = opens[i - 2];
                    var prevClose = closes[i - 2];

                    var below = currentSar < lastLow;
                    var above = currentSar > lastHigh;
                    var redCandle = lastOpen < lastClose;
                    var greenCandle = lastOpen > lastClose;
                    var priorBelow = priorSar < priorLow;
                    var priorAbove = priorSar > priorHigh;
                    var priorRedCandle = priorOpen < priorClose;
                    var priorGreenCandle = priorOpen > priorClose;
                    var prevRedCandle = prevOpen < prevClose;
                    var prevGreenCandle = prevOpen > prevClose;

                    priorRedCandle = (prevRedCandle || priorRedCandle);
                    priorGreenCandle = (prevGreenCandle || priorGreenCandle);

                    var fsar = 0;

                    if ((priorAbove && priorRedCandle) && (below && greenCandle))
                        fsar = 1;
                    else if ((priorBelow && priorGreenCandle) && (above && redCandle))
                        fsar = -1;

                    if (rsi[i] > 70 && fsar == -1)
                        result.Add(-1);
                    else if (rsi[i] < 30 && fsar == 1)
                        result.Add(1);
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
