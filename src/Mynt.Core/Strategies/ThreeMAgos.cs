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
    public class ThreeMAgos : BaseStrategy
    {
        public override string Name => "Three MAgos";
        public override int MinimumAmountOfCandles => 15;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var sma = candles.Sma(15);
            var ema = candles.Ema(15);
            var wma = candles.Wma(15);
            var closes = candles.Select(x => x.Close).ToList();

            var bars = new List<string>();

            for (int i = 0; i < candles.Count; i++)
            {
                if ((closes[i] > sma[i]) && (closes[i] > ema[i]) && (closes[i] > wma[i]))
                    bars.Add("green");
                else if ((closes[i] > sma[i]) || (closes[i] > ema[i]) || (closes[i] > wma[i]))
                    bars.Add("blue");
                else
                    bars.Add("red");

            }
            
            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(TradeAdvice.Hold);
                else if (bars[i] == "blue" && bars[i - 1] == "red")
                    result.Add(TradeAdvice.Buy);
                else if (bars[i] == "blue" && bars[i - 1] == "green")
                    result.Add(TradeAdvice.Sell);
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

