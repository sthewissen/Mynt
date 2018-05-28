using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mynt.Core.Backtester;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Data.PostgreSQL
{
    public class PostgreSqlDataStore : IDataStore
    {
        private readonly MyntDbContext _context;

        public PostgreSqlDataStore(PostgreSqlOptions options)
        {
            _context = new MyntDbContext(options.PostgreSqlConnectionString);
        }

        public async Task InitializeAsync()
        {
            // Migrate the database to the initial migration.
            await _context.Database.MigrateAsync();
        }

        public async Task<List<Trade>> GetActiveTradesAsync()
        {
            var trades = await _context.Orders.Where(x => x.IsOpen).ToListAsync();
            var items = Mapping.Mapper.Map<List<Trade>>(trades);

            return items;
        }

        public async Task<List<Trader>> GetAvailableTradersAsync()
        {
            var traders = await _context.Traders.Where(x => !x.IsBusy && !x.IsArchived).ToListAsync();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return items;
        }

        public async Task<List<Trader>> GetBusyTradersAsync()
        {
            var traders = await _context.Traders.Where(x => x.IsBusy && !x.IsArchived).ToListAsync();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return items;
        }

        public async Task SaveTradeAsync(Trade trade)
        {
            var item = Mapping.Mapper.Map<TradeAdapter>(trade);
            _context.Orders.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task SaveTraderAsync(Trader trader)
        {
            var item = Mapping.Mapper.Map<TraderAdapter>(trader);
            _context.Traders.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task SaveTradersAsync(List<Trader> traders)
        {
            var items = Mapping.Mapper.Map<List<TraderAdapter>>(traders);

            foreach(var item in items)
                _context.Traders.Add(item);
            
            await _context.SaveChangesAsync();
        }

        public async Task SaveTradesAsync(List<Trade> trades)
        {
            var items = Mapping.Mapper.Map<List<TradeAdapter>>(trades);

            foreach (var item in items)
                _context.Orders.Add(item);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Trader>> GetTradersAsync()
        {
            var traders = await _context.Traders.ToListAsync();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return items;
        }

    }
}
