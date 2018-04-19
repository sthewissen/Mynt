using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class SarStoch : BaseStrategy
    {
        public override string Name => "SAR Stoch";
        public override int MinimumAmountOfCandles => 15;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var stoch = candles.Stoch(13);
            var stochFast = candles.StochFast(13);
            var sar = candles.Sar(3);
            var highs = candles.Select(x => x.High).ToList();
            var lows = candles.Select(x => x.Low).ToList();
            var closes = candles.Select(x => x.Close).ToList();
            var opens = candles.Select(x => x.Open).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i > 2)
                {
                    var currentSar = sar[i];
                    var priorSar = sar[i - 1];
                    var lastHigh = highs[i];
                    var lastLow = lows[i];
                    var lastOpen = opens[i];
                    var lastClose = closes[i];
                    var priorHigh = highs[i - 1];
                    var priorLow = lows[i - 1];
                    var priorOpen = opens[i - 1];
                    var priorClose = closes[i - 1];
                    var prevOpen = opens[i - 2];
                    var prevClose = closes[i - 2];

                    var below = currentSar < lastLow;
                    var above = currentSar > lastHigh;
                    var redCandle = lastOpen < lastClose;
                    var greenCandle = lastOpen > lastClose;
                    var priorBelow = priorSar < priorLow;
                    var priorAbove = priorSar > priorHigh;
                    var priorRedCandle = priorOpen < priorClose;
                    var priorGreenCandle = priorOpen > priorClose;
                    var prevRedCandle = prevOpen < prevClose;
                    var prevGreenCandle = prevOpen > prevClose;

                    priorRedCandle = (prevRedCandle || priorRedCandle);
                    priorGreenCandle = (prevGreenCandle || priorGreenCandle);

                    var fsar = 0;

                    if ((priorAbove && priorRedCandle) && (below && greenCandle))
                        fsar = 1;
                    else if ((priorBelow && priorGreenCandle) && (above && redCandle))
                        fsar = -1;

                    if (fsar == -1 && (stoch.K[i] > 90 || stoch.D[i] > 90 || stochFast.K[i] > 90 || stochFast.D[i] > 90))
                        result.Add(TradeAdvice.Sell);
                    else if (fsar == 1 && (stoch.K[i] < 10 || stoch.D[i] < 10 || stochFast.K[i] < 10 || stochFast.D[i] < 10))
                        result.Add(TradeAdvice.Buy);
                    else
                        result.Add(TradeAdvice.Hold);
                }
                else
                {
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
