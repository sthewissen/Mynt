using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.liteforex.com/beginners/trading-strategies/629/
    /// </summary>
    public class AdxMomentum : ITradingStrategy
    {
        public string Name => "ADX Momentum";
                
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var adx = candles.Adx(14);
            var diPlus = candles.PlusDI(25);
            var diMinus = candles.MinusDI(25);
            var sar = candles.Sar();
            var mom = candles.Mom(14);

            for (int i = 0; i < candles.Count; i++)
            {

                if (adx[i] > 25 && mom[i] < 0 && diMinus[i] > 25 && diPlus[i] < diMinus[i])
                    result.Add(-1);
                else if (adx[i] > 25 && mom[i] > 0 && diPlus[i] > 25 && diPlus[i] > diMinus[i])
                    result.Add(1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
