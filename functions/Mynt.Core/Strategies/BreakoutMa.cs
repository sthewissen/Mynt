using System.Collections.Generic;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// Technically this is a 1d, but can be used more for e.g. 1 hour ticks.
    /// </summary>
    public class BreakoutMa : ITradingStrategy
    {
        public string Name => "Breakout MA";
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var sma20 = candles.Sma(20, CandleVariable.Low);
            var ema34 = candles.Ema(34);
            var adx = candles.Adx(13);

            for (int i = 0; i < candles.Count; i++)
            {
                if (ema34[i] > sma20[i] && adx[i] > 25)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                else if (ema34[i] < sma20[i] && adx[i] > 25)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                else
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
            }

            return result;
        }
    }
}
