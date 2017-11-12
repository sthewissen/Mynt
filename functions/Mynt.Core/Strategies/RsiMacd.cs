using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{

    public class RsiMacd : ITradingStrategy
    {

        public string Name => "RSI MACD";

        public List<Candle> Candles { get; set; }

        public RsiMacd()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var _macd = Candles.Macd(24, 52, 18);
            var _rsi = Candles.Rsi(14);

            for (int i = 0; i < Candles.Count; i++)
            {
                if (_rsi[i] > 70 && (_macd.Macd[i] - _macd.Signal[i]) < 0)
                    result.Add(-1);
                else if (_rsi[i] < 30 && (_macd.Macd[i] - _macd.Signal[i]) > 0)
                    result.Add(1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
