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

        public List<Candle> Candles { get; set; }

        public CciRsi()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var cci = Candles.Cci();
            var rsi = Candles.Rsi();

            for (int i = 0; i < Candles.Count; i++)
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
