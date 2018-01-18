using System;
using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.forexstrategiesresources.com/trend-following-forex-strategies/45-cci-and-ema/
    /// </summary>
    public class CciEma : ITradingStrategy
    {
        public string Name => "CCI EMA";

        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var cci = candles.Cci(30);
            var ema8 = candles.Ema(8);
            var ema28 = candles.Ema(28);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (cci[i] < -100 && ema8[i] > ema28[i] && ema8[i - 1] < ema28[i])
                    result.Add(1);
                else if (cci[i] > 100 && ema8[i] < ema28[i] && ema8[i - 1] > ema28[i])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
