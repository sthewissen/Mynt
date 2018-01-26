using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class Fractals : ITradingStrategy
    {
        public string Name => "Fractals";

        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            // Settings for this strat.
            var exitAfterBars = 3;
            var useLongerAverage = true;
            var noRepainting = true;

            // Our lists to hold our values
            var result = new List<ITradeAdvice>();
            var fractalPrice = new List<double>();
            var fractalAverage = new List<double>();
            var fractalTrend = new List<bool>();

            var ao = candles.AwesomeOscillator();
            var high = candles.Select(x => x.High).ToList();
            var highLowAvgs = candles.Select(x => (x.High + x.Low) / 2).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                // Calculate the price for this fractal
                if (i < 4)
                {
                    fractalPrice.Add(0);
                    fractalAverage.Add(0);
                    fractalTrend.Add(false);
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }
                else
                {
                    var fractalTop = high[i - 2] > high[i - 3] &&
                                     high[i - 2] > high[i - 4] &&
                                     high[i - 2] > high[i - 1] &&
                                     high[i - 2] > high[i];
                    var price = fractalTop ? highLowAvgs[i] : 0;
                    fractalPrice.Add(price);


                    // Calculate the avg price
                    var avg = useLongerAverage
                        ? (fractalPrice[i - 1] + fractalPrice[i - 2] + fractalPrice[i - 3]) / 3
                        : (fractalPrice[i - 1] + fractalPrice[i - 2]) / 2;
                    fractalAverage.Add(avg);

                    // Check the trend.
                    var trend = fractalAverage[i] > fractalAverage[i - 1];
                    fractalTrend.Add(trend);

                    var fractalBreakout = noRepainting
                        ? highLowAvgs[i - 1] > fractalPrice[i]
                        : highLowAvgs[i] > fractalPrice[i];

                    var tradeEntry = fractalTrend[i] && fractalBreakout;
                    var tradeExit = fractalTrend[i - exitAfterBars] && fractalTrend[i] == false;

                    if (tradeExit)
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                    else if (tradeEntry && ao[i]>0)
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                    else
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }
            }

            return result;
        }
    }
}
