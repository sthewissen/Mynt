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
    public class CciRsi : ITradingStrategy
    {
        public string Name => "CCI RSI";
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var cci = candles.Cci();
            var rsi = candles.Rsi();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else if (rsi[i] < 30 && cci[i] < -100)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                else if (rsi[i] > 70 && cci[i] > 100)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                else
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
            }

            return result;
        }
    }
}
