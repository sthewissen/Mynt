using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class TripleMa : ITradingStrategy
    {
        public string Name => "Triple MA";
        public List<Candle> Candles { get; set; }
        public List<int> Prepare()
        {
            var result = new List<int>();

            var sma20 = Candles.Sma(20);
            var sma50 = Candles.Sma(50);
            var ema11 = Candles.Ema(11);

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (ema11[i] > sma50[i] && ema11[i - 1] < sma50[i - 1])
                    result.Add(1); // A cross of the EMA and long SMA is a buy signal.
                else if ((ema11[i] < sma50[i] && ema11[i - 1] > sma50[i - 1]) || (ema11[i] < sma20[i] && ema11[i - 1] > sma20[i - 1]))
                    result.Add(-1); // As soon as our EMA crosses below an SMA its a sell signal.
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
