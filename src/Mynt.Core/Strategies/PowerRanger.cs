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
    public class PowerRanger : BaseStrategy
    {
        public override string Name => "Power Ranger";
        public override int MinimumAmountOfCandles => 10;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();
            var stoch = candles.Stoch(10);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(0);
                else
                {
                    if ((stoch.K[i] > 20 && stoch.K[i - 1] < 20) || (stoch.D[i] > 20 && stoch.D[i - 1] < 20))
                        result.Add(TradeAdvice.Buy);
                    
                    else if ((stoch.K[i] < 80 && stoch.K[i - 1] > 80) || (stoch.D[i] < 80 && stoch.D[i - 1] > 80))
                        result.Add(TradeAdvice.Sell);
                    
                    else
                        result.Add(TradeAdvice.Hold);
                }
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
