using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class EmaStochRsi : ITradingStrategy
    {
        public string Name => "EMA Stoch RSI";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var stoch = candles.Stoch(14);
            var ema5 = candles.Ema(5);
            var ema10 = candles.Ema(10);
            var rsi = candles.Rsi(14);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(0);
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
                        result.Add(1);
                    else if (ema5[i] <= ema10[i] && ema5[i - 1] > ema10[i] && rsi[i] < 50 && pointedDown)
                        result.Add(-1);
                    else
                        result.Add(0);
                }
            }

            return result;
        }
    }
}