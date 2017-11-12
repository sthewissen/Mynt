using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// This is a strategy most suited for 1 hour ticks.
    /// </summary>
    public class Momentum : ITradingStrategy
    {
        public string Name => "Momentum";

        public List<Candle> Candles { get; set; }

        public Momentum()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var sma11 = Candles.Sma(11);
            var sma21 = Candles.Sma(21);
            var mom = Candles.Mom(30);
            var rsi = Candles.Rsi();
            var closes = Candles.Select(x => x.Close).ToList();

            for (int i = 0; i < Candles.Count; i++)
            {
                if (rsi[i] <30 && mom[i] > 0 && sma11[i] > sma21[i] && closes[i] > sma21[i] && closes[i] > sma11[i])
                    result.Add(1);
                else if (rsi[i] >70 && mom[i] <0 && sma11[i] < sma21[i] && closes[i] < sma21[i] && closes[i] < sma11[i])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
