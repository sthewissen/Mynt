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
    public class FourHourEngulfing : BaseStrategy
    {
        public override string Name => "4h Engulfing";
        public override int MinimumAmountOfCandles => 50;
        public override Period IdealPeriod => Period.FourHours;

        public override List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var close = candles.Select(x => x.Close).ToList();
            var open = candles.Select(x => x.Open).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 2)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else
                {
                    // Confirmation candle is the one to buy at
                    if (close[i] > open[i] && close[i] > close[i - 1] &&
                        open[i - 2] > close[i - 2] && close[i - 1] > open[i - 1] &&
                        close[i - 1] >= open[i - 2] && close[i - 2] >= open[i - 1] && 
                        close[i - 1] - open[i - 1] > open[i - 2] - close[i - 2])
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                    else
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }
            }

            return result;
        }

        public override Candle GetSignalCandle(List<Candle> candles)
        {
            return candles[candles.Count - 2];
        }

        public override ITradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}
