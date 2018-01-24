using System.Collections.Generic;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class FisherTransform : ITradingStrategy
    {
        public string Name => "Fisher Transform";
        
        public List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();
            var fishers = candles.Fisher(10);
            var ao = candles.AwesomeOscillator();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(TradeAdvice.Hold);
                else if (fishers[i] < 0 && fishers[i - 1] > 0 && ao[i] < 0)
                    result.Add(TradeAdvice.Buy);
                else if (fishers[i] == 1)
                    result.Add(TradeAdvice.Sell);
                else
                    result.Add(TradeAdvice.Hold);
            }

            return result;
        }
    }
}
