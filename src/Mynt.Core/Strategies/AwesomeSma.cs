using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class AwesomeSma : BaseStrategy
    {
        public override string Name => "Awesome SMA";
        public override int MinimumAmountOfCandles => 40;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var ao = candles.AwesomeOscillator();
            var smaShort = candles.Sma(20);
            var smaLong = candles.Sma(40);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(TradeAdvice.Hold);
                
                else if ((ao[i] > 0 && ao[i - 1] < 0 && smaShort[i] > smaLong[i]) ||
                    (ao[i] > 0 && smaShort[i] > smaLong[i] && smaShort[i - 1] < smaLong[i - 1]))
                    result.Add(TradeAdvice.Buy);
                
                else if (smaShort[i] < smaLong[i] && smaShort[i - 1] > smaLong[i - 1])
                    result.Add(TradeAdvice.Sell);
                
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
