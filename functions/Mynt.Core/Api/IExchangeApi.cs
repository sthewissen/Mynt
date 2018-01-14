using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Core.Models;

namespace Mynt.Core.Api
{
    public interface IExchangeApi
    {
        Task<List<MarketSummary>> GetMarketSummaries();

        Task<List<Candle>> GetTickerHistory(string market, long startDate, Period period);
    }
}
