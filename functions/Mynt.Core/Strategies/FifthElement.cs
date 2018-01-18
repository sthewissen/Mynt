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
    public class FifthElement : ITradingStrategy
    {
        public string Name => "5th Element";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var macd = candles.Macd(12, 26, 9);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 4)
                    result.Add(0);
                else if ((macd.Macd[i] - macd.Signal[i]> 0) && (macd.Hist[i] > macd.Hist[i - 1] && macd.Hist[i - 1] > macd.Hist[i - 2] && macd.Hist[i - 2] > macd.Hist[i - 3] && macd.Hist[i - 3] > macd.Hist[i - 4]))
                    result.Add(1);
                else if ((macd.Macd[i] - macd.Signal[i] > 0) && (macd.Hist[i] < macd.Hist[i - 1] && macd.Hist[i - 1] < macd.Hist[i - 2] && macd.Hist[i - 2] < macd.Hist[i - 3] && macd.Hist[i - 3] < macd.Hist[i - 4]))
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
