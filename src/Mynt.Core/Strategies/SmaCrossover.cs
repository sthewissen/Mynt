using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class SmaCrossover : BaseStrategy
    {
        public override string Name => "SMA Crossover";
        public override int MinimumAmountOfCandles => 60;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var sma12 = candles.Sma(12);
            var sma16 = candles.Sma(16);
            var rsi = candles.Rsi(14);

            var crossOver = sma12.Crossover(sma16);
            var crossUnder = sma12.Crossunder(sma16);

            var startRsi = 0m;

            for (int i = 0; i < candles.Count; i++)
            {
                if (rsi[i] > startRsi)
                    startRsi = rsi[i].Value;

                // Since we look back 1 candle, the first candle can never be a signal.
                if (i == 0)
                    result.Add(TradeAdvice.Hold);

                // When the RSI has dropped 10 points from the peak, sell...
                else if (startRsi - rsi[i] > 10)
                {
                    startRsi = 0;
                    result.Add(TradeAdvice.Sell);
                }

                // When the fast SMA moves above the slow SMA, we have a positive cross-over
                else if (sma12[i] > sma16[i] && sma12[i - 1] < sma16[i - 1] && rsi[i] <= 65)
                {
                    startRsi = rsi[i].Value;
                    result.Add(TradeAdvice.Buy);
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
