using System.Collections.Generic;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.liteforex.com/beginners/trading-strategies/76/
    /// </summary>
    public class EmaAdxMacd : ITradingStrategy
    {
        public string Name => "EMA ADX MACD";

        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var ema4 = candles.Ema(4);
            var ema10 = candles.Ema(10);
            var plusDi = candles.PlusDI(28);
            var minusDi = candles.MinusDI(28);
            var macd = candles.Macd(5, 10, 4);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else if (ema4[i] < ema10[i] && ema4[i - 1] > ema10[i] && macd.Macd[i] < 0 && plusDi[i] > minusDi[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                else if (ema4[i] > ema10[i] && ema4[i - 1] < ema10[i] && macd.Macd[i] > 0 && plusDi[i] < minusDi[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                else
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
            }

            return result;
        }
    }
}
