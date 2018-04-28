using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class EmaStochRsi : BaseStrategy
    {
        public override string Name => "EMA Stoch RSI";
        public override int MinimumAmountOfCandles => 36;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var stoch = candles.Stoch(14);
            var ema5 = candles.Ema(5);
            var ema10 = candles.Ema(10);
            var rsi = candles.Rsi(14);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(TradeAdvice.Hold);
                else
                {
                    var slowk1 = stoch.K[i];
                    var slowkp = stoch.K[i - 1];
                    var slowd1 = stoch.D[i];
                    var slowdp = stoch.D[i - 1];

                    bool pointedUp = false, pointedDown = false, kUp = false, dUp = false;

                    if (slowkp < slowk1) kUp = true;
                    if (slowdp < slowd1) dUp = true;
                    if (slowkp < 80 && slowdp < 80 && kUp && dUp) pointedUp = true;
                    if (slowkp > 20 && slowdp > 20 && !kUp && !dUp) pointedDown = true;

                    if (ema5[i] >= ema10[i] && ema5[i - 1] < ema10[i] && rsi[i] > 50 && pointedUp)
                        result.Add(TradeAdvice.Buy);
                    
                    else if (ema5[i] <= ema10[i] && ema5[i - 1] > ema10[i] && rsi[i] < 50 && pointedDown)
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