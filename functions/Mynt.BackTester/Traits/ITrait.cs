using System.Collections.Generic;
using Mynt.Core.Models;

namespace Mynt.BackTester.Traits
{
    public interface ITrait
    {
        List<int> Create(List<Candle> candles);
    }
}
