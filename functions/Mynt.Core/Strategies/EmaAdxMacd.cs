using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.liteforex.com/beginners/trading-strategies/76/
    /// </summary>
    public class EmaAdxMacd : ITradingStrategy
    {
        public string Name => "EMA ADX MACD";

        public List<Candle> Candles { get; set; }

        public EmaAdxMacd()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var ema4 = Candles.Ema(4);
            var ema10 = Candles.Ema(10);
            var plusDi = Candles.PlusDI(28);
            var minusDi = Candles.MinusDI(28);
            var macd = Candles.Macd(5, 10, 4);

            for (int i = 0; i < Candles.Count; i++)
            {
                if(i==0)
                    result.Add(0);
                else if (ema4[i] < ema10[i] && ema4[i - 1] > ema10[i] && macd.Macd[i] < 0 && plusDi[i] > minusDi[i])
                    result.Add(1);
                else if (ema4[i] > ema10[i] && ema4[i - 1] < ema10[i] && macd.Macd[i] > 0 && plusDi[i] < minusDi[i])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
