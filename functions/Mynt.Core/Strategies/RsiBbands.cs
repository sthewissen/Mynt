using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.tradingview.com/script/uCV8I4xA-Bollinger-RSI-Double-Strategy-by-ChartArt-v1-1/
    /// </summary>
    public class RsiBbands : ITradingStrategy
    {
        public string Name => "RSI Bbands";

        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var rsi = candles.Rsi(6);
            var bbands = candles.Bbands(200);
            var closes = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                else if (rsi[i-1] > 50 && rsi[i] <= 50 && closes[i - 1] < bbands.UpperBand[i - 1] && closes[i] > bbands.UpperBand[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                else if (rsi[i - 1] < 50 && rsi[i] >= 50 && closes[i-1] < bbands.LowerBand[i-1] && closes[i] > bbands.LowerBand[i])
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                else
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));

            }

            return result;
        }
    }
}
