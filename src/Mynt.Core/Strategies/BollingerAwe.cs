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
    public class BollingerAwe : BaseStrategy
    {
        public override string Name => "BollingerAwe";
        public override int MinimumAmountOfCandles => 50;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var closes = candles.Select(x => x.Close).ToList();
            var bb = candles.Bbands(20);
            var fastMa = candles.Ema(3);
            var hl1 = candles.Select(x => (x.High + x.Low) / 2).ToList().Sma(5);
            var hl2 = candles.Select(x => (x.High + x.Low) / 2).ToList().Sma(34);
            var ao = new List<int>();
            var macd = candles.Macd();

            for (int i = 0; i < hl1.Count; i++)
            {
                if (i > 0)
                {
                    if (hl1[i-1].HasValue && hl2[i-1].HasValue && hl1[i].HasValue && hl2[i].HasValue)
                        ao.Add(hl1[i].Value - hl2[i].Value >= 0
                            ? hl1[i].Value - hl2[i].Value > hl1[i - 1].Value - hl2[i - 1].Value ? 1 : 2
                            : hl1[i].Value - hl2[i].Value > hl1[i - 1].Value - hl2[i - 1].Value ? -1 : -2);
                    else
                        ao.Add(0);
                }
                else
                    ao.Add(0);
            }

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(TradeAdvice.Hold);
                else
                {
                    if (closes[i] > bb.MiddleBand[i] && // Closed above the bollinger band
                        Math.Abs(ao[i]) == 1 &&
                        macd.Macd[i] > macd.Signal[i]&&
                        fastMa[i] > bb.MiddleBand[i] &&
                        fastMa[i-1] < bb.MiddleBand[i])
                        result.Add(TradeAdvice.Buy);
                    else if (closes[i] < bb.MiddleBand[i] && // Closed above the bollinger band
                        Math.Abs(ao[i]) == 2 &&
                        fastMa[i] < bb.MiddleBand[i] &&
                        fastMa[i - 1] > bb.MiddleBand[i])
                        result.Add(TradeAdvice.Sell);
                    else
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
