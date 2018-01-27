using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class MacdSma : ITradingStrategy
    {
        public string Name => "MACD SMA";

        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var macd = candles.Macd();
            var fastMa = candles.Sma(12);
            var slowMa = candles.Sma(26);
            var sma200 = candles.Sma(200);

            var closes = candles.Select(x => x.Close).ToList();
            
            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 25)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else if(slowMa[i] < sma200[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                else if (macd.Hist[i] >0 && macd.Hist[i-1] < 0 && macd.Macd[i] > 0 && fastMa[i] > slowMa[i] && closes[i-26] > sma200[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
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
