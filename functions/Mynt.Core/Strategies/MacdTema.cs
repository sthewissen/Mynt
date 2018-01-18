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
    public class MacdTema : ITradingStrategy
    {
        public string Name => "MACD TEMA";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();
            var macd = candles.Macd(12, 26, 9);
            var tema = candles.Tema(50);

            var close = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (tema[i] < close[i] && tema[i-1] > close[i-1] && macd.Macd[i] > 0 && macd.Macd[i-1] < 0 )
                    result.Add(1);
                else if (tema[i] > close[i] && tema[i - 1] < close[i - 1] && macd.Macd[i] < 0 && macd.Macd[i - 1] > 0)
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
