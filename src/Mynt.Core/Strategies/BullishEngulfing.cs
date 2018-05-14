using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class BullishEngulfing : BaseStrategy
    {
        public override string Name => "Bullish Engulfing";
        public override int MinimumAmountOfCandles => 11;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice> { TradeAdvice.Buy };

            var rsi = candles.Rsi(11);
            var close = candles.Select(x => x.Close).ToList();
            var high = candles.Select(x => x.High).ToList();
            var low = candles.Select(x => x.Low).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(TradeAdvice.Hold);
                else if (rsi[i] < 40 && low[i-1] < close[i] && high[i-1] < high[i] && high[i-1] < close[i])
                    result.Add(TradeAdvice.Buy);
                else if (high[i-1] > close[i] && low[i-1] < low[i] && close[i-1] < low[i] && rsi[i] > 60)
                    result.Add(TradeAdvice.Sell);
                else
                    result.Add(TradeAdvice.Hold);
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
