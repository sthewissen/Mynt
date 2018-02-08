using System.Collections.Generic;
using Mynt.Core.Models;

namespace Mynt.DataAccess.Interfaces
{
    public interface ICandleStorage
    {
        List<Candle> GetCandles(string symbol);
    }
}
