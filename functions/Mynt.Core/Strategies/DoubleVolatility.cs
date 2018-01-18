using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.liteforex.com/beginners/trading-strategies/detail/679/
    /// </summary>
    public class DoubleVolatility : ITradingStrategy
    {
        public string Name => "Double Volatility";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var sma5High = candles.Sma(5, CandleVariable.High);
            var sma20High = candles.Sma(20, CandleVariable.High);
            var sma20Low = candles.Sma(20, CandleVariable.Low);
            var closes = candles.Select(x => x.Close).ToList();
            var opens = candles.Select(x => x.Open).ToList();
            var rsi = candles.Rsi(11);

            for (int i = 0; i < candles.Count; i++)
            {
                if(i<1)
                    result.Add(0);
                else if (sma5High[i] > sma20High[i] && rsi[i] > 65 && Math.Abs(opens[i] - closes[i]) / Math.Abs(opens[i-1] - closes[i-1]) < 2)
                    result.Add(1);
                else if (sma5High[i] < sma20Low[i] && rsi[i] < 35)
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}