using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class FreqTrade : BaseStrategy
    {
        public override string Name => "FreqTrade";
        public override int MinimumAmountOfCandles => 15;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var rsi = candles.Rsi(14);
            var adx = candles.Adx(14);
            var plusDi = candles.PlusDI(14);
            var minusDi = candles.MinusDI(14);
            var fast = candles.StochFast();

            for (int i = 0; i < candles.Count; i++)
            {
                if ((rsi[i] < 35 && fast.D[i] < 35 && adx[i] > 30 && plusDi[i] > 0.5m) || (adx[i] > 65 && plusDi[i] > 0.5m))
                    result.Add(TradeAdvice.Buy);

                else if ((adx[i] > 10 && minusDi[i] > 0 && ((rsi[i] > 70 && rsi[i - 1] < 70) || (fast.D[i] > 70 && fast.D[i - 1] < 70))) || (adx[i] > 70 && minusDi[i] > 0.5m))
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
