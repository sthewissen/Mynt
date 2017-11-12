using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.liteforex.com/beginners/trading-strategies/595/
    /// </summary>
    public class AwesomeMacd : ITradingStrategy
    {
        public string Name => "Awesome MACD";

        public List<Candle> Candles { get; set; }

        public AwesomeMacd()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var ao = Candles.AwesomeOscillator();
            var macd = Candles.Macd(5, 7, 4);

            for (int i = 0; i < Candles.Count; i++)
            {
                if(i==0)
                    result.Add(0);
                else if (ao[i] < 0 && ao[i-1] > 0 && macd.Hist[i] < 0)
                    result.Add(-1);
                else if (ao[i] > 0 && ao[i -1] < 0 &&  macd.Hist[i] > 0)
                    result.Add(1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
