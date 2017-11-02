using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class WvfExtended : ITradingStrategy
    {
        public string Name => "Williams Vix Fix (Extended)";

        public List<Candle> Candles { get; set; }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var ao = Candles.AwesomeOscillator();
            var close = Candles.Select(x => x.Close).ToList();
            var high = Candles.Select(x => x.High).ToList();
            var low = Candles.Select(x => x.Low).ToList();
            var open = Candles.Select(x => x.Open).ToList();

            var stochRsi = Candles.StochRsi(14);

            var wvfs = new List<double>();
            var standardDevs = new List<double>();
            var rangeHighs = new List<double>();
            var upperRanges = new List<double?>();

            var pd = 22; // LookBack Period Standard Deviation High
            var bbl = 20; // Bollinger Band Length
            var mult = 2.0; // Bollinger Band Standard Deviation Up
            var lb = 50; // Look Back Period Percentile High
            var ph = .85; // Highest Percentile - 0.90=90%, 0.95=95%, 0.99=99%
            var pl = 1.01; // Lowest Percentile - 1.10=90%, 1.05=95%, 1.01=99%
            var ltLB = 40; // Long-Term Look Back Current Bar Has To Close Below This Value OR Medium Term")
            var mtLB = 14; // Medium-Term Look Back Current Bar Has To Close Below This Value OR Long Term")
            var str = 3; // Entry Price Action Strength--Close > X Bars Back")

            for (int i = 0; i < Candles.Count; i++)
            {
                var itemsToPick = i < pd - 1 ? i + 1 : pd;
                var indexToStartFrom = i < pd - 1 ? 0 : i - pd;

                var highestClose = Candles.Skip(indexToStartFrom).Take(itemsToPick).Select(x => x.Close).Max();
                var wvf = ((highestClose - Candles[i].Low) / (highestClose)) * 100;

                // Calculate the WVF
                wvfs.Add(wvf);

                double standardDev = 0;

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

            for (int i = 0; i < Candles.Count; i++)
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

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i < ltLB)
                    result.Add(0);
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

                    if ((filteredAlert || aggressiveAlert))
                        result.Add(1);
                    else if (stochRsi.K[i] > 80 && stochRsi.K[i] > stochRsi.D[i] && stochRsi.K[i - 1] < stochRsi.D[i - 1] && ao[i] < 0 && ao[i - 1] > 0)
                        result.Add(-1);
                    else
                        result.Add(0);
                }
            }

            return result;
        }

        private double GetStandardDeviation(List<double> doubleList)
        {
            double average = doubleList.Average();
            double sumOfDerivation = 0;
            foreach (double value in doubleList)
            {
                sumOfDerivation += (value) * (value);
            }
            double sumOfDerivationAverage = sumOfDerivation / (doubleList.Count - 1);
            return Math.Sqrt(sumOfDerivationAverage - (average * average));
        }
    }
}
