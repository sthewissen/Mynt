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
    public class PowerRanger : ITradingStrategy
    {
        public string Name => "Power Ranger";
        
        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();
            var stoch = candles.Stoch(10);

            for (int i = 0; i < candles.Count; i++)
            {
                if (i < 1)
                    result.Add(0);
                else
                {
                    if ((stoch.K[i] > 20 && stoch.K[i - 1] < 20) || (stoch.D[i] > 20 && stoch.D[i - 1] < 20))
                        result.Add(1);
                    else if ((stoch.K[i] < 80 && stoch.K[i - 1] > 80) || (stoch.D[i] < 80 && stoch.D[i - 1] > 80))
                        result.Add(-1);
                    else
                        result.Add(0);
                }
            }

            return result;
        }
    }
}
