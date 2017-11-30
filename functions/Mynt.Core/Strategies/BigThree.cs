using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class BigThree : ITradingStrategy
    {
        public string Name => "Big Three";

        public List<Candle> Candles { get; set; }

        public BigThree()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();
            
            var sma20 = Candles.Sma(20);
            var sma40 = Candles.Sma(40);
            var sma80 = Candles.Sma(80);

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i < 2)
                {
                    result.Add(0);
                }
                else
                {
                    var lastIsGreen = Candles[i].Close > Candles[i].Open;
                    var previousIsRed = Candles[i - 1].Close < Candles[i - 1].Open;
                    var beforeIsGreen = Candles[i - 2].Close > Candles[i - 2].Open;

                    var highestSma = new List<double?> { sma20[i], sma40[i], sma80[i] }.Max();

                    var lastAboveSma = Candles[i].Close > highestSma && Candles[i].High > highestSma &&
                                       Candles[i].Low > highestSma && Candles[i].Open > highestSma;

                    var previousAboveSma = Candles[i - 1].Close > highestSma && Candles[i - 1].High > highestSma &&
                                       Candles[i - 1].Low > highestSma && Candles[i - 1].Open > highestSma;

                    var beforeAboveSma = Candles[i - 2].Close > highestSma && Candles[i - 2].High > highestSma &&
                                       Candles[i - 2].Low > highestSma && Candles[i - 2].Open > highestSma;

                    var allAboveSma = lastAboveSma && previousAboveSma && beforeAboveSma;
                    var hitsAnSma = (sma80[i] < Candles[i].High && sma80[i] > Candles[i].Low);

                    if (lastIsGreen && previousIsRed && beforeIsGreen && allAboveSma && sma20[i] > sma40[i] && sma20[i] > sma80[i])
                        result.Add(1);
                    else if (hitsAnSma)
                        result.Add(-1);
                    else
                        result.Add(0);
                }
            }

            return result;
        }
    }
}
