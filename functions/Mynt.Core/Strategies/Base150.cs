using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.liteforex.com/beginners/trading-strategies/623/
    /// </summary>
    public class Base150 : ITradingStrategy
    {
        public string Name => "Base 150";

        public List<Candle> Candles { get; set; }

        public Base150()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var sma6 = Candles.Sma(6);
            var sma25 = Candles.Sma(25);
            var sma150 = Candles.Sma(150);
            var sma365 = Candles.Sma(365);

            for (int i = 0; i < Candles.Count; i++)
            {
                if (i == 0)
                {
                    result.Add(0);
                }
                else
                {
                    if (sma6[i] > sma150[i]
                        && sma6[i] > sma365[i]
                        && sma25[i] > sma150[i]
                        && sma25[i] > sma365[i]
                        && (sma6[i - 1] < sma150[i]
                        || sma6[i - 1] < sma365[i]
                        || sma25[i - 1] < sma150[i]
                        || sma25[i - 1] < sma365[i]))
                        result.Add(1);
                    if (sma6[i] < sma150[i]
                        && sma6[i] < sma365[i]
                        && sma25[i] < sma150[i]
                        && sma25[i] < sma365[i]
                        && (sma6[i - 1] > sma150[i]
                        || sma6[i - 1] > sma365[i]
                        || sma25[i - 1] > sma150[i]
                        || sma25[i - 1] > sma365[i]))
                        result.Add(-1);
                    else
                        result.Add(0);
                }
            }

            return result;
        }
    }
}
