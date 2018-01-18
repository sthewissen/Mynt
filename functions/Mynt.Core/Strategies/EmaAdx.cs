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
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var emaFast = candles.Ema(12);
            var emaShort = candles.Ema(36);
            var adx = candles.Adx();

            for (int i = 0; i < candles.Count; i++)
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
