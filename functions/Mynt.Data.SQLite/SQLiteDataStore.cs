using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Data.SQLite
{
    public class SQLiteDataStore : IDataStore
    {
        private SQLiteContext _context;

        public SQLiteDataStore(SQLiteOptions options)
        {
            // TODO: Get some decent instance of the SQLiteContext.
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask; // TODO
        }

        public Task<List<Trade>> GetActiveTradesAsync()
        {
            var trades = _context.Trades.Where(x => x.IsOpen).ToList();
            var items = Mapping.Mapper.Map<List<Trade>>(trades);

            return Task.FromResult(items);
        }

        public Task<List<Trader>> GetAvailableTradersAsync()
        {
            var traders = _context.Traders.Where(x => !x.IsBusy).ToList();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return Task.FromResult(items);
        }

        public Task<List<Trader>> GetBusyTradersAsync()
        {
            var traders = _context.Traders.Where(x => x.IsBusy).ToList();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return Task.FromResult(items);
        }

        public Task SaveTradeAsync(Trade trade)
        {
            throw new NotImplementedException();
        }

        public Task SaveTraderAsync(Trader trader)
        {
            throw new NotImplementedException();
        }

        public Task SaveTradersAsync(List<Trader> traders)
        {
            throw new NotImplementedException();
        }

        public Task SaveTradesAsync(List<Trade> trades)
        {
            throw new NotImplementedException();
        }

        public Task<List<Trader>> GetTradersAsync()
        {
            var traders = _context.Traders.ToList();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return Task.FromResult(items);
        }
    }
}
