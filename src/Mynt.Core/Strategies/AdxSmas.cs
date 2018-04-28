using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class AdxSmas : BaseStrategy
    {
        public override string Name => "ADX Smas";
        public override int MinimumAmountOfCandles => 14;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var sma6 = candles.Sma(3);
            var sma40 = candles.Sma(10);
            var adx = candles.Adx(14);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                {
                    result.Add(0);
                }
                else
                {
                    var sixCross = ((sma6[i - 1] < sma40[i] && sma6[i] > sma40[i]) ? 1 : 0);
                    var fortyCross = ((sma40[i - 1] < sma6[i] && sma40[i] > sma6[i]) ? 1 : 0);

                    if (adx[i] > 25 && sixCross == 1)
                        result.Add(TradeAdvice.Buy);
                    else if (adx[i] < 25 && fortyCross == 1)
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
