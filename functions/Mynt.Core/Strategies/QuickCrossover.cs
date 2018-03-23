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
    public class QuickCrossover : BaseStrategy
    {
        public override string Name => "Quick Crossover";
        public override int MinimumAmountOfCandles => 50;
        public override Period IdealPeriod => Period.Hour;

        public override List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();
            var fastMa = candles.Ema(5);
            var slowMa = candles.Ema(10);
            var hl2 = candles.Select(x => (x.High + x.Low) / 2).ToList();
            var rsi = hl2.Rsi(10);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else
                {
                    if (fastMa[i] > slowMa[i] && fastMa[i-1] < slowMa[i-1] &&
                        rsi[i] > 50 && rsi[i-1] < 50)
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
