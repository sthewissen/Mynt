using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// http://www.profitf.com/forex-strategies/ema-adx-15-min-system/
    /// </summary>
    public class EmaAdxF : ITradingStrategy
    {
        public string Name => "EMA ADX F";

        public List<Candle> Candles { get; set; }

        public EmaAdxF()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var closes = Candles.Select(x => x.Close).ToList();
            var ema9 = Candles.Ema(9);
            var adx = Candles.Adx(14);
            var minusDI = Candles.MinusDI(14);
            var plusDI = Candles.PlusDI(14);

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (ema9[i] < closes[i] && plusDI[i] > 20 && plusDI[i] > minusDI[i])
                    result.Add(1);
                else if (ema9[i] > closes[i] && minusDI[i] > 20 && plusDI[i] < minusDI[i])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
