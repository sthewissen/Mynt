using System;
using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    /// <summary>
    /// https://www.liteforex.com/beginners/trading-strategies/830/
    /// </summary>
    public class CciScalper : ITradingStrategy
    {
        public string Name => "CCI Scalper";
        public List<Candle> Candles { get; set; }

        public CciScalper()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            if (Candles.Count < 200)
                throw new Exception("Need larger data set: (200 min).");

            var cci = Candles.Cci(200);
            var ema10 = Candles.Ema(10);
            var ema21 = Candles.Ema(21);
            var ema50 = Candles.Ema(50);

            for (int i = 0; i < Candles.Count; i++)
            {
                if (cci[i] > 0 && ema10[i] > ema21[i] && ema10[i] > ema50[i])
                    result.Add(1);
                else if (cci[i] < 0 && ema10[i] < ema21[i] && ema10[i] < ema50[i])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
