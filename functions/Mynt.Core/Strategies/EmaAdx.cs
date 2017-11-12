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
    public class EmaAdx : ITradingStrategy
    {
        public string Name => "EMA ADX";

        public List<Candle> Candles { get; set; }

        public EmaAdx()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var emaFast = Candles.Ema(12);
            var emaShort = Candles.Ema(36);
            var adx = Candles.Adx();

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (emaFast[i] > emaShort[i] && emaFast[i - 1] < emaShort[i] && adx[i] < 20)
                    result.Add(1);
                else if (emaFast[i] < emaShort[i] && emaFast[i - 1] > emaShort[i] && adx[i] >= 20)
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
