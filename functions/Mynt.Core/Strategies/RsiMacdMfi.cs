using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class RsiMacdMfi : ITradingStrategy
    {
        public string Name => "RSI MACD MFI";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var macd = candles.Macd(5,10,4);
            var rsi = candles.Rsi(16);
            var mfi = candles.Mfi();
            var ao = candles.AwesomeOscillator();

            var close = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {       
                    if (mfi[i] <30 && rsi[i] < 45 && ao[i] > 0)
                        result.Add(1);
                    else if ( mfi[i] > 30 && rsi[i] > 45 && ao[i] < 0)
                        result.Add(-1);
                    else
                        result.Add(0);
             }

            return result;
        }
    }
}