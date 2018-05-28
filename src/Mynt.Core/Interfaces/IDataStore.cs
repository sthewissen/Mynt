using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Backtester.Models;
using Mynt.Core.Backtester;
using Mynt.Core.Models;

namespace Mynt.Core.Interfaces
{
    public interface IDataStore
    {
        // Initialization
        Task InitializeAsync();
 
        // Trade/order related methods
        Task<List<Trade>> GetActiveTradesAsync();
        Task SaveTradesAsync(List<Trade> trades);
        Task SaveTradeAsync(Trade trade);

        // Trader related methods
        Task<List<Trader>> GetTradersAsync();
        Task<List<Trader>> GetBusyTradersAsync();
        Task<List<Trader>> GetAvailableTradersAsync();
        Task SaveTradersAsync(List<Trader> traders);
        Task SaveTraderAsync(Trader trader);
    }
}