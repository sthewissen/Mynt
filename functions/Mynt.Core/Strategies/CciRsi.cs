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
    public class CciRsi : ITradingStrategy
    {
        public string Name => "CCI RSI";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var cci = candles.Cci();
            var rsi = candles.Rsi();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (rsi[i] < 30 && cci[i] < -100)
                    result.Add(1);
                else if (rsi[i] > 70 && cci[i] > 100)
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
