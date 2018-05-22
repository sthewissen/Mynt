using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class SmaCrossover : BaseStrategy
    {
        public override string Name => "SMA Crossover";
        public override int MinimumAmountOfCandles => 26;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var sma12 = candles.Sma(12);
            var sma26 = candles.Sma(26);

            for (int i = 0; i < candles.Count; i++)
            {
                // Since we look back 1 candle, the first candle can never be a signal.
                if (i == 0)
                    result.Add(TradeAdvice.Hold);
                // When the slow SMA moves above the fast SMA, we have a negative cross-over
                else if (sma12[i] < sma26[i] && sma12[i - 1] > sma26[i - 1])
                    result.Add(TradeAdvice.Sell);
                // When the fast SMA moves above the slow SMA, we have a positive cross-over
                else if (sma12[i] > sma26[i] && sma12[i - 1] < sma26[i - 1])
                    result.Add(TradeAdvice.Buy);
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
