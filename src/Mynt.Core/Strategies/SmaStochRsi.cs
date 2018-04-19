using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class SmaStochRsi : BaseStrategy
    {
        public override string Name => "SMA Stoch RSI";
        public override int MinimumAmountOfCandles => 150;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var price = candles.Select(x => x.Close).ToList();
            var stoch = candles.Stoch(8);
            var sma150 = candles.Sma(150);
            var rsi = candles.Rsi(3);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(TradeAdvice.Hold);
                else
                {
                    if (price[i] > sma150[i] && stoch.K[i] > 70 && rsi[i] < 20 && stoch.K[i] > stoch.D[i])
                        result.Add(TradeAdvice.Buy);
                    else if (price[i] < sma150[i] && stoch.K[i] > 70 && rsi[i] > 80 && stoch.K[i] < stoch.D[i])
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

