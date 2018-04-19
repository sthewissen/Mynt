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
    public class FifthElement : BaseStrategy
    {
        public override string Name => "5th Element";
        public override int MinimumAmountOfCandles => 30;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var macd = candles.Macd(12, 26, 9);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 4)
                    result.Add(TradeAdvice.Hold);

                else if ((macd.Macd[i] - macd.Signal[i]> 0) && (macd.Hist[i] > macd.Hist[i - 1] && macd.Hist[i - 1] > macd.Hist[i - 2] && macd.Hist[i - 2] > macd.Hist[i - 3] && macd.Hist[i - 3] > macd.Hist[i - 4]))
                    result.Add(TradeAdvice.Buy);

                else if ((macd.Macd[i] - macd.Signal[i] > 0) && (macd.Hist[i] < macd.Hist[i - 1] && macd.Hist[i - 1] < macd.Hist[i - 2] && macd.Hist[i - 2] < macd.Hist[i - 3] && macd.Hist[i - 3] < macd.Hist[i - 4]))
                    result.Add(TradeAdvice.Sell);
               
                else
                    result.Add(TradeAdvice.Hold);
            }

            return result;
        }

        public override Candle GetSignalCandle(List<Candle> candles)
        {
            return candles.Last();
        }

        public override TradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}
