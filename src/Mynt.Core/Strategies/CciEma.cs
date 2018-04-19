using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class CciEma : BaseStrategy
    {
        public override string Name => "CCI EMA";
        public override int MinimumAmountOfCandles => 30;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var cci = candles.Cci(30);
            var ema8 = candles.Ema(8);
            var ema28 = candles.Ema(28);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(TradeAdvice.Hold);
                
                else if (cci[i] < -100 && ema8[i] > ema28[i] && ema8[i - 1] < ema28[i])
                    result.Add(TradeAdvice.Buy);
                
                else if (cci[i] > 100 && ema8[i] < ema28[i] && ema8[i - 1] > ema28[i])
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
