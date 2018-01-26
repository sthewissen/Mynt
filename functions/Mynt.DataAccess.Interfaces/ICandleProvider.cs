using System.Collections.Generic;
using Mynt.Core.Models;

namespace Mynt.DataAccess.Interfaces
{
    public interface ICandleProvider
    {
        List<Candle> GetCandles(string symbol);
    }
}
