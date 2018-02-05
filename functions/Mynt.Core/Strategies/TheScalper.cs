using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class TheScalper : BaseStrategy
    {
        public override string Name => "The Scalper";
        public override int MinimumAmountOfCandles => 210;
        public override Period IdealPeriod => Period.Hour;

        public override List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var stoch = candles.Stoch();
            var sma200 = candles.Sma(200);
            var closes = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else
                {
                    if (sma200[i] < closes[i] && // Candles above the SMA
                        stoch.K[i - 1] <= stoch.D[i - 1] && // K below 20, oversold
                        stoch.K[i] > stoch.D[i] &&
                        stoch.D[i - 1] < 20 &&
                        stoch.K[i - 1] < 20 // && // K below 20, oversold
                        )
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                    else
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }
            }

            return result;
        }

        public override ITradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}

