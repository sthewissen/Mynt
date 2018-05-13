using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class Replex : BaseStrategy
    {
        public override string Name => "Replex";
        public override int MinimumAmountOfCandles => 20;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var rsi = candles.Rsi(14);
            var bbands = candles.Bbands(20);
            var stoch = candles.Stoch();
            var stochRsi = candles.StochRsi(fastKPeriod: 3);
            var close = candles.Close();
            var open = candles.Open();

            for (int i = 0; i < candles.Count; i++)
            {
                if (rsi[i] > 70 && stoch.K[i] > 80 && close[i] > open[i] && stochRsi.K[i] > 80 &&
                    stoch.K[i] >= stoch.D[i] && stochRsi.K[i] >= stochRsi.D[i] && close[i] > (bbands.UpperBand[i] + ((bbands.UpperBand[i] - bbands.MiddleBand[i]) * 0.05m)))
                    result.Add(TradeAdvice.Sell);
                else if (rsi[i] < 30 && stoch.K[i] < 20 && close[i] < open[i] && stochRsi.K[i] < 20 &&
                    stoch.K[i] <= stoch.D[i] && stochRsi.K[i] <= stochRsi.D[i] && close[i] < (bbands.LowerBand[i] - ((bbands.MiddleBand[i] - bbands.LowerBand[i]) * 0.05m)))
                    result.Add(TradeAdvice.Buy);
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
