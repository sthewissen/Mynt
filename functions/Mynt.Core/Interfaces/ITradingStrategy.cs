using System.Collections.Generic;
using Mynt.Core.Models;

namespace Mynt.Core.Interfaces
{
    public interface ITradingStrategy
    {
        string Name { get;  }

        List<ITradeAdvice> Prepare(List<Candle> candles);
    }
}
