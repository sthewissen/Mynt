using System.Collections.Generic;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class TripleMa : ITradingStrategy
    {
        public string Name => "Triple MA";

        public List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var sma20 = candles.Sma(20);
            var sma50 = candles.Sma(50);
            var ema11 = candles.Ema(11);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(TradeAdvice.Hold);
                else if (ema11[i] > sma50[i] && ema11[i - 1] < sma50[i - 1])
                    result.Add(TradeAdvice.Buy); // A cross of the EMA and long SMA is a buy signal.
                else if ((ema11[i] < sma50[i] && ema11[i - 1] > sma50[i - 1]) || (ema11[i] < sma20[i] && ema11[i - 1] > sma20[i - 1]))
                    result.Add(TradeAdvice.Sell); // As soon as our EMA crosses below an SMA its a sell signal.
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
