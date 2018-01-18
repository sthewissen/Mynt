using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class SmaStochRsi : ITradingStrategy
    {
        public string Name => "SMA Stoch RSI";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var price = candles.Select(x => x.Close).ToList();
            var stoch = candles.Stoch(8);
            var sma150 = candles.Sma(150);
            var rsi = candles.Rsi(3);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(0);
                else
                {
                    if (price[i] > sma150[i] && stoch.K[i] > 70 && rsi[i] < 20 && stoch.K[i] > stoch.D[i])
                        result.Add(1);
                    else if (price[i] < sma150[i] && stoch.K[i] > 70 && rsi[i] > 80 && stoch.K[i] < stoch.D[i])
                        result.Add(-1);
                    else
                        result.Add(0);
                }
            }

            return result;
        }
    }
}

