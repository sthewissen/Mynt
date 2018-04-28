using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class RsiBbands : BaseStrategy
    {
        public override string Name => "RSI Bbands";
        public override int MinimumAmountOfCandles => 200;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var rsi = candles.Rsi(6);
            var bbands = candles.Bbands(200);
            var closes = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(TradeAdvice.Hold);
                else if (rsi[i - 1] > 50 && rsi[i] <= 50 && closes[i - 1] < bbands.UpperBand[i - 1] && closes[i] > bbands.UpperBand[i])
                    result.Add(TradeAdvice.Sell);
                else if (rsi[i - 1] < 50 && rsi[i] >= 50 && closes[i - 1] < bbands.LowerBand[i - 1] && closes[i] > bbands.LowerBand[i])
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
