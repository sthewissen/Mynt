using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class BigThree : BaseStrategy
    {
        public override string Name => "Big Three";
        public override int MinimumAmountOfCandles => 100;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();
            
            var sma20 = candles.Sma(20);
            var sma40 = candles.Sma(40);
            var sma80 = candles.Sma(80);
            
            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 2)
                {
                    result.Add(TradeAdvice.Hold);
                }
                else
                {
                    var lastIsGreen = candles[i].Close > candles[i].Open;
                    var previousIsRed = candles[i - 1].Close < candles[i - 1].Open;
                    var beforeIsGreen = candles[i - 2].Close > candles[i - 2].Open;

                    var highestSma = new List<decimal?> { sma20[i], sma40[i], sma80[i] }.Max();

                    var lastAboveSma = candles[i].Close > highestSma && candles[i].High > highestSma &&
                                       candles[i].Low > highestSma && candles[i].Open > highestSma;

                    var previousAboveSma = candles[i - 1].Close > highestSma && candles[i - 1].High > highestSma &&
                                       candles[i - 1].Low > highestSma && candles[i - 1].Open > highestSma;

                    var beforeAboveSma = candles[i - 2].Close > highestSma && candles[i - 2].High > highestSma &&
                                       candles[i - 2].Low > highestSma && candles[i - 2].Open > highestSma;

                    var allAboveSma = lastAboveSma && previousAboveSma && beforeAboveSma;
                    var hitsAnSma = (sma80[i] < candles[i].High && sma80[i] > candles[i].Low);

                    if (lastIsGreen && previousIsRed && beforeIsGreen && allAboveSma && sma20[i] > sma40[i] && sma20[i] > sma80[i])
                        result.Add(TradeAdvice.Buy);
                    else if (hitsAnSma)
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
    }
}
