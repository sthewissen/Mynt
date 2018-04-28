using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class EmaAdxMacd : BaseStrategy
    {
        public override string Name => "EMA ADX MACD";
        public override int MinimumAmountOfCandles => 30;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var ema4 = candles.Ema(4);
            var ema10 = candles.Ema(10);
            var plusDi = candles.PlusDI(28);
            var minusDi = candles.MinusDI(28);
            var macd = candles.Macd(5, 10, 4);

            for (int i = 0; i < candles.Count; i++)
            {
                if(i==0)
                    result.Add(TradeAdvice.Hold);
                else if (ema4[i] < ema10[i] && ema4[i - 1] > ema10[i] && macd.Macd[i] < 0 && plusDi[i] > minusDi[i])
                    result.Add(TradeAdvice.Buy);
                else if (ema4[i] > ema10[i] && ema4[i - 1] < ema10[i] && macd.Macd[i] > 0 && plusDi[i] < minusDi[i])
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
