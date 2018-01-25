using System.Collections.Generic;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.liteforex.com/beginners/trading-strategies/595/
    /// </summary>
    public class AwesomeMacd : ITradingStrategy
    {
        public string Name => "Awesome MACD";
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var ao = candles.AwesomeOscillator();
            var macd = candles.Macd(5, 7, 4);

            for (int i = 0; i < candles.Count; i++)
            {
                if(i == 0)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else if (ao[i] < 0 && ao[i-1] > 0 && macd.Hist[i] < 0)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                else if (ao[i] > 0 && ao[i -1] < 0 &&  macd.Hist[i] > 0)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                else
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
            }

            return result;
        }
    }
}
