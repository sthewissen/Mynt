using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// This is a strategy most suited for 1 hour ticks.
    /// </summary>
    public class Momentum : BaseStrategy
    {
        public override string Name => "Momentum";
        public override int MinimumAmountOfCandles => 30;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();
            var sma11 = candles.Sma(11);
            var sma21 = candles.Sma(21);
            var mom = candles.Mom(30);
            var rsi = candles.Rsi();
            var closes = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (rsi[i] < 30 && mom[i] > 0 && sma11[i] > sma21[i] && closes[i] > sma21[i] && closes[i] > sma11[i])
                    result.Add(TradeAdvice.Buy);
                
                else if (rsi[i] > 70 && mom[i] < 0 && sma11[i] < sma21[i] && closes[i] < sma21[i] && closes[i] < sma11[i])
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
