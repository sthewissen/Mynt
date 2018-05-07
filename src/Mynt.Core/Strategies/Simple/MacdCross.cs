using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies.Simple
{
    public class MacdCross : BaseStrategy, INotificationTradingStrategy
    {
        public override string Name => "MACD X";
        public override int MinimumAmountOfCandles => 50;
        public override Period IdealPeriod => Period.Hour;

        public string BuyMessage => "MACD: *Oversold*\nTrend reversal to the *upside* is near.";
        public string SellMessage => "MACD: *Overbought*\nTrend reversal to the *downside* is near.";

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var macd = candles.Macd();
            var crossUnder = macd.Macd.Crossunder(macd.Signal);
            var crossOver = macd.Macd.Crossover(macd.Signal);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(TradeAdvice.Hold);
                else if (macd.Macd[i] > 0 && crossUnder[i])
                    result.Add(TradeAdvice.Sell);
                else if (macd.Macd[i] < 0 && crossOver[i])
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
