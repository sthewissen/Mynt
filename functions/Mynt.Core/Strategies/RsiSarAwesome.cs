using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// http://www.tradeforextrading.com/parabolic-sar-indicator/awesome-oscillator-indicator-relative-strength-index-rsi-indicator-forex-trading-system.htm
    /// </summary>
    public class RsiSarAwesome : ITradingStrategy
    {
        public string Name => "RSI SAR Awesome";
        
        public List<ITradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<ITradeAdvice>();

            var sar = candles.Sar();
            var rsi = candles.Rsi(5);
            var ao = candles.AwesomeOscillator();

            var close = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i >= 2)
                {
                    var currentSar = sar[i];
                    var priorSar = sar[i - 1];
                    
                    if (currentSar < close[i] && priorSar > close[i] && ao[i] > 0 && rsi[i] > 50)
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Buy));
                    else if (currentSar > close[i] && priorSar < close[i] && ao[i] < 0 && rsi[i] < 50)
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Sell));
                    else
                        result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }
                else
                {
                    result.Add(new SimpleTradeAdvice(TradeAdvice.Hold));
                }
            }

            return result;
        }
    }
}