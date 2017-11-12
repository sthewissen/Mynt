using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class RsiMacdAwesome : ITradingStrategy
    {
        public string Name => "RSI MACD Awesome";

        public List<Candle> Candles { get; set; }

        public RsiMacdAwesome()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var macd = Candles.Macd(5,10,4);
            var rsi = Candles.Rsi(16);
            var ao = Candles.AwesomeOscillator();

            var close = Candles.Select(x => x.Close).ToList();

            for (int i = 0; i < Candles.Count; i++)
            {       
                    if (macd.Hist[i] < 0 && ao[i] > 0 && rsi[i] < 45)
                        result.Add(1);
                    else if (macd.Hist[i] > 0 && ao[i] < 0 && rsi[i] > 45)
                        result.Add(-1);
                    else
                        result.Add(0);
             }

            return result;
        }
    }
}