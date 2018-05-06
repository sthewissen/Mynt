using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies.Simple
{
    public class SmaGoldenCross : BaseStrategy, INotificationTradingStrategy
    {
        public override string Name => "SMA 50/200 Golden Cross";
        public override int MinimumAmountOfCandles => 200;
        public override Period IdealPeriod => Period.Hour;

        public string BuyMessage => "Golden Cross: *Crossover*\nTrend reversal to the *upside* is near.";
        public string SellMessage => "Golden Cross: *Crossunder*\nTrend reversal to the *downside* is near.";

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            {
                var result = new List<TradeAdvice>();

                var sma50 = candles.Sma(50);
                var sma200 = candles.Sma(200);
                var crossUnder = sma50.Crossunder(sma200);
                var crossOver = sma50.Crossover(sma200);

                for (int i = 0; i < candles.Count; i++)
                {
                    // Since we look back 1 candle, the first candle can never be a signal.
                    if (i == 0)
                        result.Add(TradeAdvice.Hold);
                    // When the slow SMA moves above the fast SMA, we have a negative cross-over
                    else if (crossUnder[i])
                        result.Add(TradeAdvice.Sell);
                    // When the fast SMA moves above the slow SMA, we have a positive cross-over
                    else if (crossOver[i])
                        result.Add(TradeAdvice.Buy);
                    else
                        result.Add(TradeAdvice.Hold);
                }

                return result;
            }
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