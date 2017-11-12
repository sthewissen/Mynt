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
    /// http://best-binary-options-strategy.com/binary-option-trading-using-the-adx-and-ema-cross-system/
    /// </summary>
    public class EmaAdxSmall : ITradingStrategy
    {
        public string Name => "EMA ADX Small";

        public List<Candle> Candles { get; set; }

        public EmaAdxSmall()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var closes = Candles.Select(x => x.Close).ToList();
            var emaFast = Candles.Ema(3);
            var emaSlow = Candles.Ema(10);
            var minusDI = Candles.MinusDI(14);
            var plusDI = Candles.PlusDI(14);

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (emaFast[i] > emaSlow[i] && (emaFast[i - 1] < emaSlow[i - 1] || plusDI[i - 1] < minusDI[i - 1]) && plusDI[i] > 20 && plusDI[i] > minusDI[i])
                    result.Add(1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
