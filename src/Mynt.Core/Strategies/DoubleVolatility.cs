using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.liteforex.com/beginners/trading-strategies/detail/679/
    /// </summary>
    public class DoubleVolatility : BaseStrategy
    {
        public override string Name => "Double Volatility";
        public override int MinimumAmountOfCandles => 20;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var sma5High = candles.Sma(5, CandleVariable.High);
            var sma20High = candles.Sma(20, CandleVariable.High);
            var sma20Low = candles.Sma(20, CandleVariable.Low);
            var closes = candles.Select(x => x.Close).ToList();
            var opens = candles.Select(x => x.Open).ToList();
            var rsi = candles.Rsi(11);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(TradeAdvice.Hold);

                else if (sma5High[i] > sma20High[i] && rsi[i] > 65 && Math.Abs(opens[i - 1] - closes[i - 1]) > 0 && Math.Abs(opens[i] - closes[i]) / Math.Abs(opens[i - 1] - closes[i - 1]) < 2)
                    result.Add(TradeAdvice.Buy);

                else if (sma5High[i] < sma20Low[i] && rsi[i] < 35)
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