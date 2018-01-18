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
    public class ThreeMAgos : ITradingStrategy
    {
        public string Name => "Three MAgos";

        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

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
                    result.Add(0);
                else if (bars[i] == "blue" && bars[i - 1] == "red")
                    result.Add(1);
                else if (bars[i] == "blue" && bars[i - 1] == "green")
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}

