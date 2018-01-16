using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Core.Models;

namespace Mynt.Core.Api
{
    public interface IExchangeApi
    {
        Task<string> Buy(string market, double quantity, double rate);

        Task<string> Sell(string market, double quantity, double rate);

        Task<double> GetBalance(string currency);

        Task<List<MarketSummary>> GetMarketSummaries();

        Task<List<OpenOrder>> GetOpenOrders(string market);

        Task<Ticker> GetTicker(string market);

        Task<List<Candle>> GetTickerHistory(string market, long startDate, Period period);
    }
}
