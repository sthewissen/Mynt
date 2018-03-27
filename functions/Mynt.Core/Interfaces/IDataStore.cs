using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Core.Models;

namespace Mynt.Core.Interfaces
{
    public interface IDataStore
    {
        // Trade/order related methods
        Task<List<Trade>> GetActiveTradesAsync();
        Task SaveTradesAsync(List<Trade> trades);
        Task SaveTradeAsync(Trade trade);

        // Trader related methods
        Task<List<Trader>> GetBusyTradersAsync();
        Task<List<Trader>> GetAvailableTradersAsync();
        Task SaveTradersAsync(List<Trader> traders);
        Task SaveTraderAsync(Trader trader);
    }

    public interface IDataStoreOptions
    {
        
    }
}
