using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class AwesomeSma : ITradingStrategy
    {
        public string Name => "Awesome SMA";
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var ao = candles.AwesomeOscillator();
            var smaShort = candles.Sma(20);
            var smaLong = candles.Sma(40);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else if ((ao[i] > 0 && ao[i - 1] < 0 && smaShort[i] > smaLong[i]) ||
                    (ao[i] > 0 && smaShort[i] > smaLong[i] && smaShort[i - 1] < smaLong[i - 1]))
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
