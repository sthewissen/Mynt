using System.Collections.Generic;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class FaMaMaMa : ITradingStrategy
    {
        public string Name => "FAMAMAMA";
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var mama = candles.Mama(0.5, 0.05);
            var fama = candles.Mama(0.25, 0.025);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else if (fama.Mama[i] > mama.Mama[i] && fama.Mama[i - 1] < mama.Mama[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                else if (fama.Mama[i] < mama.Mama[i] && fama.Mama[i - 1] > mama.Mama[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                else
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
            }

            return result;
        }
    }
}