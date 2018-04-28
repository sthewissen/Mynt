using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.liteforex.com/beginners/trading-strategies/830/
    /// </summary>
    public class CciScalper : BaseStrategy
    {
        public override string Name => "CCI Scalper";
        public override int MinimumAmountOfCandles => 200;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            if (candles.Count < 200)
                throw new Exception("Need larger data set: (200 min).");

            var cci = candles.Cci(200);
            var ema10 = candles.Ema(10);
            var ema21 = candles.Ema(21);
            var ema50 = candles.Ema(50);

            for (int i = 0; i < candles.Count; i++)
            {
                if (cci[i] > 0 && ema10[i] > ema21[i] && ema10[i] > ema50[i])
                    result.Add(TradeAdvice.Buy);
                else if (cci[i] < 0 && ema10[i] < ema21[i] && ema10[i] < ema50[i])
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
