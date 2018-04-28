using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class SmaSar : BaseStrategy
    {
        public override string Name => "SMA SAR";
        public override int MinimumAmountOfCandles => 60;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var sma = candles.Sma(60);
            var sar = candles.Sar();
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

                    if (closes[i] > sma[i] && fsar == 1)
                        result.Add(TradeAdvice.Buy);
                    else if (closes[i] < sma[i] && fsar == -1)
                        result.Add(TradeAdvice.Sell);
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
