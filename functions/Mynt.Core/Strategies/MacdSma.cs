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
    public class MacdSma : ITradingStrategy
    {
        public string Name => "MACD SMA";
        public List<Candle> Candles { get; set; }
        public List<int> Prepare()
        {
            var result = new List<int>();

            var macd = Candles.Macd();
            var fastMa = Candles.Sma(12);
            var slowMa = Candles.Sma(26);
            var sma200 = Candles.Sma(200);

            var closes = Candles.Select(x => x.Close).ToList();
            
            for (int i = 0; i < Candles.Count; i++)
            {
                if (i < 25)
                    result.Add(0);
                else if(slowMa[i] < sma200[i])
                    result.Add(-1);
                else if (macd.Hist[i] >0 && macd.Hist[i-1] < 0 && macd.Macd[i] > 0 && fastMa[i] > slowMa[i] && closes[i-26] > sma200[i])
                    result.Add(1);
                else
                    result.Add(0);

            }

            return result;
        }
    }
}
