using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{

    public class RsiMacd : ITradingStrategy
    {
        public string Name => "RSI MACD";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();

            var _macd = candles.Macd(24, 52, 18);
            var _rsi = candles.Rsi(14);

            for (int i = 0; i < candles.Count; i++)
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
