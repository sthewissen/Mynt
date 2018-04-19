using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class Base150 : BaseStrategy
    {
        public override string Name => "Base 150";
        public override int MinimumAmountOfCandles => 365;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var sma6 = candles.Sma(6);
            var sma25 = candles.Sma(25);
            var sma150 = candles.Sma(150);
            var sma365 = candles.Sma(365);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                {
                    result.Add(0);
                }
                else
                {
                    if (sma6[i] > sma150[i]
                        && sma6[i] > sma365[i]
                        && sma25[i] > sma150[i]
                        && sma25[i] > sma365[i]
                        && (sma6[i - 1] < sma150[i]
                        || sma6[i - 1] < sma365[i]
                        || sma25[i - 1] < sma150[i]
                        || sma25[i - 1] < sma365[i]))
                        result.Add(TradeAdvice.Buy);
                    if (sma6[i] < sma150[i]
                        && sma6[i] < sma365[i]
                        && sma25[i] < sma150[i]
                        && sma25[i] < sma365[i]
                        && (sma6[i - 1] > sma150[i]
                        || sma6[i - 1] > sma365[i]
                        || sma25[i - 1] > sma150[i]
                        || sma25[i - 1] > sma365[i]))
                        result.Add(TradeAdvice.Sell);
                    else
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
