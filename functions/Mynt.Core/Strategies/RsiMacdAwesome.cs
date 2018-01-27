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
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var macd = candles.Macd(5,10,4);
            var rsi = candles.Rsi(16);
            var ao = candles.AwesomeOscillator();

            var close = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {       
                    if (macd.Hist[i] < 0 && ao[i] > 0 && rsi[i] < 45)
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                    else if (macd.Hist[i] > 0 && ao[i] < 0 && rsi[i] > 45)
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                    else
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
             }

            return result;
        }

        public ITradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}