using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.tradingview.com/script/zopumZ8a-Bollinger-RSI-Double-Strategy-by-ChartArt/
    /// </summary>
    public class BbandRsi : ITradingStrategy
    {
        public string Name => "BBand RSI";

        public List<Candle> Candles { get; set; }

        public BbandRsi()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var currentPrices = Candles.Select(x => x.Close).ToList();
            var bbands = Candles.Bbands(20);
            var rsi = Candles.Rsi(16);
            
            for (int i = 0; i < Candles.Count; i++)
            {
                if (i == 0)
                    result.Add(0);
                else if (rsi[i] < 45 && currentPrices[i-1] < bbands.LowerBand[i-1] && currentPrices[i] >= bbands.LowerBand[i])
                    result.Add(1);
                else if (rsi[i] > 55 && currentPrices[i-1] > bbands.UpperBand[i] && currentPrices[i] <= bbands.UpperBand[i])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
