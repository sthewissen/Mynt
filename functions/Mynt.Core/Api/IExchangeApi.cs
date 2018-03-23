using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Core.Enums;
using Mynt.Core.Models;

namespace Mynt.Core.Api
{
    public interface IExchangeApi
    {
        Task<string> Buy(string market, double quantity, double rate);

        Task<string> Sell(string market, double quantity, double rate);

        Task<AccountBalance> GetBalance(string currency);

        Task<List<MarketSummary>> GetMarketSummaries();

        Task<Order> GetOrder(string orderId, string market);

        Task<List<OpenOrder>> GetOpenOrders(string market);

        Task CancelOrder(string orderId, string market);

        Task<Ticker> GetTicker(string market);

        Task<OrderBook> GetOrderBook(string market);

        Task<List<Candle>> GetTickerHistory(string market, Period period, DateTime startDate, DateTime? endDate = null);
        
        Task<List<Candle>> GetTickerHistory(string market, Period period, int length);
    }
}
