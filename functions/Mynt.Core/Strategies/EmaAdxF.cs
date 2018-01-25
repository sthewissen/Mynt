using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// http://www.profitf.com/forex-strategies/ema-adx-15-min-system/
    /// </summary>
    public class EmaAdxF : ITradingStrategy
    {
        public string Name => "EMA ADX F";
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var closes = candles.Select(x => x.Close).ToList();
            var ema9 = candles.Ema(9);
            var adx = candles.Adx(14);
            var minusDI = candles.MinusDI(14);
            var plusDI = candles.PlusDI(14);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else if (ema9[i] < closes[i] && plusDI[i] > 20 && plusDI[i] > minusDI[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                else if (ema9[i] > closes[i] && minusDI[i] > 20 && plusDI[i] < minusDI[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                else
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
            }

            return result;
        }
    }
}
