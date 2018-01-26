using System.Collections.Generic;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// http://www.binarytribune.com/forex-trading-strategies/combining-average-directional-movement-index-and-emas/
    /// </summary>
    public class AdxSmas : ITradingStrategy
    {
        public string Name => "ADX Smas";
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var sma6 = candles.Sma(3);
            var sma40 = candles.Sma(10);
            var adx = candles.Adx(14);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                {
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }
                else
                {
                    var sixCross = ((sma6[i - 1] < sma40[i] && sma6[i] > sma40[i]) ? 1 : 0);
                    var fortyCross = ((sma40[i - 1] < sma6[i] && sma40[i] > sma6[i]) ? 1 : 0);

                    if (adx[i] > 25 && sixCross == 1)
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                    else if (adx[i] < 25 && fortyCross == 1)
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                    else
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }
            }

            return result;
        }
    }
}
