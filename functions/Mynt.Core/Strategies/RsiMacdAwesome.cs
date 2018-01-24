using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class RsiMacdAwesome : ITradingStrategy
    {
        public string Name => "RSI MACD Awesome";
        
        public List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var macd = candles.Macd(5,10,4);
            var rsi = candles.Rsi(16);
            var ao = candles.AwesomeOscillator();

            var close = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {       
                    if (macd.Hist[i] < 0 && ao[i] > 0 && rsi[i] < 45)
                        result.Add(TradeAdvice.Buy);
                    else if (macd.Hist[i] > 0 && ao[i] < 0 && rsi[i] > 45)
                        result.Add(TradeAdvice.Sell);
                    else
                        result.Add(TradeAdvice.Hold);
             }

            return result;
        }
    }
}