using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class GoldenCross : BaseStrategy
    {
        public override string Name => "Golden Cross";
        public override int MinimumAmountOfCandles => 202;
        public override Period IdealPeriod => Period.Day;

        public override List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            {
                var result = new List<ITradeAdvice>();

                var sma50 = candles.Sma(50);
                var sma200 = candles.Sma(200);

                for (int i = 0; i < candles.Count; i++)
                {
                    // Since we look back 1 candle, the first candle can never be a signal.
                    if (i == 0)
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                    // When the fast SMA moves above the slow SMA, we have a positive cross-over
                    else if (sma50[i] < sma200[i] && sma50[i - 1] > sma200[i])
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                    // When the slow SMA moves above the fast SMA, we have a negative cross-over
                    else if (sma50[i] > sma200[i] && sma50[i - 1] < sma200[i])
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                    else
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }

                return result;
            }
        }

        public override Candle GetSignalCandle(List<Candle> candles)
        {
            return candles.Last();
        }

        public override ITradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}