using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// http://www.binarytribune.com/forex-trading-strategies/combining-average-directional-movement-index-and-emas/
    /// </summary>
    public class AdxSmas : ITradingStrategy
    {
        public string Name => "ADX Smas";

        public List<Candle> Candles { get; set; }

        public AdxSmas()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var sma6 = Candles.Sma(3);
            var sma40 = Candles.Sma(10);
            var adx = Candles.Adx(14);

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i == 0)
                {
                    result.Add(0);
                }
                else
                {
                    var sixCross = ((sma6[i - 1] < sma40[i] && sma6[i] > sma40[i]) ? 1 : 0);
                    var fortyCross = ((sma40[i - 1] < sma6[i] && sma40[i] > sma6[i]) ? 1 : 0);

                    if (adx[i] > 25 && sixCross == 1)
                        result.Add(1);
                    else if (adx[i] < 25 && fortyCross == 1)
                        result.Add(-1);
                    else
                        result.Add(0);
                }
            }

            return result;
        }
    }
}
