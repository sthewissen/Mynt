using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class BuyAndHold : ITradingStrategy
    {
        public string Name => "Buy & Hold";

        public List<int> Prepare(List<Candle> candles)
        {
            var result = new List<int>();
            result.Add(1);
            result.AddRange(new int[candles.Count - 1]);
            return result;
        }
    }
}
