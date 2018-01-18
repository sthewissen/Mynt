using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var rsi = candles.Rsi(6);
            var bbands = candles.Bbands(200);
            var closes = candles.Select(x => x.Close).ToList();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(0);
                else if (rsi[i-1] > 50 && rsi[i] <= 50 && closes[i - 1] < bbands.UpperBand[i - 1] && closes[i] > bbands.UpperBand[i])
                    result.Add(-1);
                else if (rsi[i - 1] < 50 && rsi[i] >= 50 && closes[i-1] < bbands.LowerBand[i-1] && closes[i] > bbands.LowerBand[i])
                    result.Add(1);
                else
                    result.Add(0);

            }

            return result;
        }
    }
}
