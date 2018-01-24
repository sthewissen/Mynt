using System.Collections.Generic;
using Mynt.Core.Enums;
using Mynt.Core.Models;

namespace Mynt.Core.Interfaces
{
    public interface ITradingStrategy
    {
        string Name { get;  }

        List<TradeAdvice> Prepare(List<Candle> candles);
    }
}
