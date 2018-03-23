using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class CloudBreaker : BaseStrategy
    {
        public override string Name => "Cloud Breaker";
        public override int MinimumAmountOfCandles => 130;
        public override Period IdealPeriod => Period.Hour;

        public override List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var closes = candles.Select(x => x.Close).ToList();
            var ichi = candles.Ichimoku();
            var rsi = candles.Rsi();
            var displacement = 30;

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < displacement + 1)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else
                {
                    // Cloud break 1
                    if (ichi.SenkouSpanA[i] > ichi.SenkouSpanB[i] &&
                        closes[i] > ichi.SenkouSpanA[i - displacement] &&
                        closes[i - 1] < ichi.SenkouSpanA[i - displacement + 1])
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));

                    else if (ichi.SenkouSpanB[i] > ichi.SenkouSpanA[i] &&
                        closes[i] > ichi.SenkouSpanB[i - displacement] &&
                        closes[i - 1] < ichi.SenkouSpanB[i - displacement + 1])
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));

                    else if (ichi.SenkouSpanA[i] > ichi.SenkouSpanB[i] &&
                            closes[i] < ichi.SenkouSpanA[i - displacement] &&
                            closes[i - 1] > ichi.SenkouSpanA[i - displacement + 1])
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));

                    else if (ichi.SenkouSpanB[i] > ichi.SenkouSpanA[i] &&
                        closes[i] < ichi.SenkouSpanB[i - displacement] &&
                        closes[i - 1] > ichi.SenkouSpanB[i - displacement + 1])
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));

                    else
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }
            }

            return result;
        }

        public override Candle GetSignalCandle(List<Candle> candles)
        {
            return candles.Last();
        }

        public override ITradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}
