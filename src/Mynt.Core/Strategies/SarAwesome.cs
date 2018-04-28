using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class SarAwesome : BaseStrategy
    {
        public override string Name => "SAR Awesome";
        public override int MinimumAmountOfCandles => 35;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

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
                        result.Add(TradeAdvice.Buy);
                    else if ((currentSar < lastLow) && (priorSar < lastLow) && (earlierSar < lastLow) && ao[i] < 0 && ema5[i] > close[i])
                        result.Add(TradeAdvice.Sell);
                    else
                        result.Add(TradeAdvice.Hold);
                }
                else
                {
                    result.Add(TradeAdvice.Hold);
                }
            }

            return result;
        }

        public override Candle GetSignalCandle(List<Candle> candles)
        {
            return candles.Last();
        }

        public override TradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}