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

        public List<Candle> Candles { get; set; }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var sma = Candles.Sma(100);
            var closes = Candles.Select(x => x.Close).ToList();
            var adx = Candles.Adx();
            var tema = Candles.Tema(4);
            var mfi = Candles.Mfi(14);
            var sar = Candles.Sar(0.02, 0.22);

            var cci = Candles.Cci(5);
            var stoch = Candles.StochFast();
            var bbandsLower = Candles.Bbands().LowerBand;

            for (int i = 0; i < Candles.Count; i++)
            {
                if (closes[i] < sma[i] && cci[i] < -100 && stoch.D[i] < 20 &&
                    adx[i] > 20 && mfi[i] < 30 && tema[i] <= bbandsLower[i])
                    result.Add(1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
