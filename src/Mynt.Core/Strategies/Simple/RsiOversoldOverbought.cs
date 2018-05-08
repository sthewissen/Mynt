using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies.Simple
{
    public class RsiOversoldOverbought : BaseStrategy, INotificationTradingStrategy
    {
        public override string Name => "RSI Oversold/Overbought";
        public override int MinimumAmountOfCandles => 200;
        public override Period IdealPeriod => Period.Hour;

        public string BuyMessage => "RSI: *Oversold*\nTrend reversal to the *upside* is near.";
        public string SellMessage => "RSI: *Overbought*\nTrend reversal to the *downside* is near.";

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var rsi = candles.Rsi(14);
            var crossOver = rsi.Crossover(30);
            var crossUnder = rsi.Crossunder(70);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(TradeAdvice.Hold);
                else if (crossUnder[i])
                    result.Add(TradeAdvice.Sell);
                else if (crossOver[i])
                    result.Add(TradeAdvice.Buy);
                else
                    result.Add(TradeAdvice.Hold);
            }

            return result;
        }

        public override TradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }

        public override Candle GetSignalCandle(List<Candle> candles)
        {
            return candles.Last();
        }
    }
}
    