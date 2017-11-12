using System.Collections.Generic;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{

    /// Short-term indicator.
    /// Typically close these after a 15pt rise/fall
    public class StochAdx : ITradingStrategy
    {
        public string Name => "Stoch ADX";

        public List<Candle> Candles { get; set; }

        public StochAdx()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var stoch = Candles.Stoch(13);
            var adx = Candles.Adx(14);
            var bearBull = Candles.BearBull();

            for (int i = 0; i < Candles.Count; i++)
            {
                if (adx[i] > 50 && (stoch.K[i] > 90 || stoch.D[i] > 90) && bearBull[i] == -1)
                    result.Add(-1);
                else if (adx[i] < 20 && (stoch.K[i] < 10 || stoch.D[i] < 10) && bearBull[i] == 1)
                    result.Add(1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
