using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
	public class QuickSma : BaseStrategy
    {
        public override string Name => "Quick SMA";
        public override int MinimumAmountOfCandles => 20;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

			var sma1 = candles.Sma(12);
            var sma2 = candles.Sma(18);
			var closes = candles.Close();
            var crossOver = sma1.Crossover(sma2);
			var crossUnder = sma2.Crossunder(closes);
            
            for (int i = 0; i < candles.Count; i++)
            {
                if (crossOver[i])
                    result.Add(TradeAdvice.Buy);
                else if (crossUnder[i])
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
