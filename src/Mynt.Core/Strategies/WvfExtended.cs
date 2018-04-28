using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class WvfExtended : BaseStrategy
    {
        public override string Name => "Williams Vix Fix (Extended)";
        public override int MinimumAmountOfCandles => 40;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var close = candles.Select(x => x.Close).ToList();
            var high = candles.Select(x => x.High).ToList();
            var low = candles.Select(x => x.Low).ToList();
            var open = candles.Select(x => x.Open).ToList();
            var rsi = candles.Rsi(20);
            var ema = rsi.Ema(10);

            var stochRsi = candles.StochRsi(14);

            var wvfs = new List<decimal>();
            var standardDevs = new List<decimal>();
            var rangeHighs = new List<decimal>();
            var upperRanges = new List<decimal?>();

            var pd = 22; // LookBack Period Standard Deviation High
            var bbl = 20; // Bollinger Band Length
            var mult = 2.0m; // Bollinger Band Standard Deviation Up
            var lb = 50; // Look Back Period Percentile High
            var ph = .85m; // Highest Percentile - 0.90=90%, 0.95=95%, 0.99=99%
            var pl = 1.01m; // Lowest Percentile - 1.10=90%, 1.05=95%, 1.01=99%
            var ltLB = 40; // Long-Term Look Back Current Bar Has To Close Below This Value OR Medium Term")
            var mtLB = 14; // Medium-Term Look Back Current Bar Has To Close Below This Value OR Long Term")
            var str = 3; // Entry Price Action Strength--Close > X Bars Back")

            for (int i = 0; i < candles.Count; i++)
            {
                var itemsToPick = i < pd - 1 ? i + 1 : pd;
                var indexToStartFrom = i < pd - 1 ? 0 : i - pd;

                var highestClose = candles.Skip(indexToStartFrom).Take(itemsToPick).Select(x => x.Close).Max();
                var wvf = ((highestClose - candles[i].Low) / (highestClose)) * 100;

                // Calculate the WVF
                wvfs.Add(wvf);

                decimal standardDev = 0;

                if (wvfs.Count > 1)
                {
                    if (wvfs.Count < bbl)
                        standardDev = mult * GetStandardDeviation(wvfs.Take(bbl).ToList());
                    else
                        standardDev = mult * GetStandardDeviation(wvfs.Skip(wvfs.Count - bbl).Take(bbl).ToList());
                }

                // Also calculate the standard deviation.
                standardDevs.Add(standardDev);
            }

            var midLines = wvfs.Sma(bbl);

            for (int i = 0; i < candles.Count; i++)
            {
                var upperBand = midLines[i] + standardDevs[i];

                if (upperBand.HasValue)
                    upperRanges.Add(upperBand.Value);
                else
                    upperRanges.Add(null);

                var itemsToPickRange = i < lb - 1 ? i + 1 : lb;
                var indexToStartFromRange = i < lb - 1 ? 0 : i - lb;

                var rangeHigh = wvfs.Skip(indexToStartFromRange).Take(itemsToPickRange).Max() * ph;
                rangeHighs.Add(rangeHigh);
            }

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < ltLB)
                    result.Add(TradeAdvice.Hold);
                else
                {
                    //Filtered Bar Criteria
                    var upRange = low[i] > low[i - 1] && close[i] > high[i - 1];
                    var upRangeAggr = close[i] > close[i - 1] && close[i] > open[i - 1];

                    //Filtered Criteria
                    var filtered = ((wvfs[i - 1] >= upperRanges[i - 1] || wvfs[i - 1] >= rangeHighs[i - 1]) &&
                                    (wvfs[i] < upperRanges[i] && wvfs[i] < rangeHighs[i]));
                    var filteredAggr = (wvfs[i - 1] >= upperRanges[i - 1] || wvfs[i - 1] >= rangeHighs[i - 1]) &&
                                       !(wvfs[i] < upperRanges[i] && wvfs[i] < rangeHighs[i]);

                    var filteredAlert = upRange && close[i] > close[i - str] &&
                                        (close[i] < close[i - ltLB] || close[i] < close[i - mtLB]) && filtered;
                    var aggressiveAlert = upRangeAggr && close[i] > close[i - str] &&
                                          (close[i] < close[i - ltLB] || close[i] < close[i - mtLB]) && filteredAggr;

                    if ((filteredAlert || aggressiveAlert) && rsi[i] > ema[i] && rsi[i - 1] < ema[i - 1])
                        result.Add(TradeAdvice.Buy);
                    else if (stochRsi.K[i] > 80 && stochRsi.K[i] > stochRsi.D[i] && stochRsi.K[i - 1] < stochRsi.D[i - 1])
                        result.Add(TradeAdvice.Sell);
                    else
                        result.Add(TradeAdvice.Hold);
                }
            }

            return result;
        }

        public override TradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }

        public override Candle GetSignalCandle(List<Candle> candles)
        {
            return candles.Last();
        }

        private decimal GetStandardDeviation(List<decimal> decimalList)
        {
            decimal average = decimalList.Average();
            decimal sumOfDerivation = 0;
            foreach (decimal value in decimalList)
            {
                sumOfDerivation += (value) * (value);
            }
            decimal sumOfDerivationAverage = sumOfDerivation / (decimalList.Count - 1);
            return (decimal)Math.Sqrt((double)(sumOfDerivationAverage - (average * average)));
        }
    }
}
