using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// This is a strategy most suited for 1 hour ticks.
    /// </summary>
    public class Momentum : ITradingStrategy
    {
        public string Name => "Momentum";
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var sma11 = candles.Sma(11);
            var sma21 = candles.Sma(21);
            var mom = candles.Mom(30);
            var rsi = candles.Rsi();
            var closes = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (rsi[i] <30 && mom[i] > 0 && sma11[i] > sma21[i] && closes[i] > sma21[i] && closes[i] > sma11[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                else if (rsi[i] >70 && mom[i] <0 && sma11[i] < sma21[i] && closes[i] < sma21[i] && closes[i] < sma11[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                else
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
            }

            return result;
        }
    }
}
