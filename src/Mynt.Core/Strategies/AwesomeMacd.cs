using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class AwesomeMacd : BaseStrategy
    {
        public override string Name => "Awesome MACD";
        public override int MinimumAmountOfCandles => 40;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var ao = candles.AwesomeOscillator();
            var macd = candles.Macd(5, 7, 4);

            for (int i = 0; i < candles.Count; i++)
            {
                if(i==0)
                    result.Add(TradeAdvice.Hold);
                
                else if (ao[i] < 0 && ao[i-1] > 0 && macd.Hist[i] < 0)
                    result.Add(TradeAdvice.Sell);
                
                else if (ao[i] > 0 && ao[i -1] < 0 &&  macd.Hist[i] > 0)
                    result.Add(TradeAdvice.Buy);
                
                else
                    result.Add(TradeAdvice.Hold);
            }

            return result;
        }

        public override TradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }

        public override Candle GetSignalCandle(List<Candle> candles)
        {
            return candles.Last();
        }
    }
}
