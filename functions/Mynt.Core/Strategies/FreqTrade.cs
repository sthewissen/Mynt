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
    public class FreqTrade : ITradingStrategy
    {
        public string Name => "FreqTrade";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var sma = candles.Sma(100);
            var closes = candles.Select(x => x.Close).ToList();
            var adx = candles.Adx();
            var tema = candles.Tema(4);
            var mfi = candles.Mfi(14);
            var sar = candles.Sar(0.02, 0.22);

            var cci = candles.Cci(5);
            var stoch = candles.StochFast();
            var bbandsLower = candles.Bbands().MiddleBand;
            var fishers = candles.Fisher();

            for (int i = 0; i < candles.Count; i++)
            {
                if (closes[i] < sma[i] && cci[i] < -100 && stoch.D[i] < 20 && fishers[i] < 0 &&
                    adx[i] > 20 && mfi[i] < 30 && tema[i] <= bbandsLower[i])
                    result.Add(1);
                else if (fishers[i] == 1)
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
