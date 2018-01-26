using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class SmaStochRsi : ITradingStrategy
    {
        public string Name => "SMA Stoch RSI";
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var price = candles.Select(x => x.Close).ToList();
            var stoch = candles.Stoch(8);
            var sma150 = candles.Sma(150);
            var rsi = candles.Rsi(3);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else
                {
                    if (price[i] > sma150[i] && stoch.K[i] > 70 && rsi[i] < 20 && stoch.K[i] > stoch.D[i])
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                    else if (price[i] < sma150[i] && stoch.K[i] > 70 && rsi[i] > 80 && stoch.K[i] < stoch.D[i])
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                    else
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }
            }

            return result;
        }
    }
}

