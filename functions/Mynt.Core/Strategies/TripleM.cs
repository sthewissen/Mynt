using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class TripleM : BaseStrategy
    {
        public override string Name => "Triple M";
        public override int MinimumAmountOfCandles => 105;
        public override Period IdealPeriod => Period.Hour;

        public override List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var opens = candles.Select(x => x.Open).ToList();
            var closes = candles.Select(x => x.Close).ToList();
            var ema7 = candles.Ema(7);
            var ema25 = candles.Ema(25);
            var ema99 = candles.Ema(99);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else
                {
                    if (closes[i] > opens[i] && // Closed green
                        ((ema7[i] > ema99[i] && ema7[i - 1] < ema99[i - 1]) ||
                        (ema7[i] > ema25[i] && ema7[i - 1] < ema25[i - 1])) && // Crossover 
                        ema7[i] > ema25[i] && // Above the 25 EMA
                        ema7[i] > ema99[i] && // Above the 99 EMA
                        closes[i] > ema7[i] &&
                        opens[i] > ema7[i])
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                    else
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }
            }



            return result;
        }

        public override Candle GetSignalCandle(List<Candle> candles)
        {
            return candles.Last();
        }

        public override ITradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}
