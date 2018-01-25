using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// http://best-binary-options-strategy.com/binary-option-trading-using-the-adx-and-ema-cross-system/
    /// </summary>
    public class EmaAdxSmall : ITradingStrategy
    {
        public string Name => "EMA ADX Small";
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var closes = candles.Select(x => x.Close).ToList();
            var emaFast = candles.Ema(3);
            var emaSlow = candles.Ema(10);
            var minusDI = candles.MinusDI(14);
            var plusDI = candles.PlusDI(14);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else if (emaFast[i] > emaSlow[i] && (emaFast[i - 1] < emaSlow[i - 1] || plusDI[i - 1] < minusDI[i - 1]) && plusDI[i] > 20 && plusDI[i] > minusDI[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                else
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
            }

            return result;
        }
    }
}
