using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.UnitTests.TradeManagers
{
    public class InMemoryDataStore : IDataStore
    {
        public List<Trade> Trades { get; set; }
        public List<Trader> Traders { get; set; }
        public static InMemoryDataStore Instance { get; set; }

        public InMemoryDataStore()
        {
            Trades = new List<Trade>();
            Traders = new List<Trader>();
            Instance = this;
        }

        public Task<List<Trade>> GetActiveTradesAsync()
        {
            return Task.FromResult(Trades.Where(x => x.IsOpen).ToList());
        }

        public Task<List<Trader>> GetAvailableTradersAsync()
        {
            return Task.FromResult(Traders.Where(x => !x.IsBusy).ToList());
        }

        public Task<List<Trader>> GetBusyTradersAsync()
        {
            return Task.FromResult(Traders.Where(x => x.IsBusy).ToList());
        }

        public Task<List<Trader>> GetTradersAsync()
        {
            return Task.FromResult(Traders);
        }

        public Task InitializeAsync()
        {
            // No init code needed...
            return Task.FromResult(true);
        }

        public Task SaveTradeAsync(Trade trade)
        {
            var items = Trades.Where(x => x.TradeId == trade.TradeId).ToList();

            if (items.Count > 0)
            {
                foreach (var item in items)
                    Trades.Remove(item);

                Trades.Add(trade);
            }
            else
            {
                Trades.Add(trade);
            }

            return Task.FromResult(true);
        }

        public Task SaveTraderAsync(Trader trader)
        {
            var items = Traders.Where(x => x.Identifier == trader.Identifier).ToList();

            if (items.Count > 0)
            {
                foreach (var item in items)
                    Traders.Remove(item);

                Traders.Add(trader);
            }
            else
            {
                Traders.Add(trader);
            }

            return Task.FromResult(true);
        }

        public async Task SaveTradersAsync(List<Trader> traders)
        {
            foreach (var item in traders)
                await SaveTraderAsync(item);
        }

        public async Task SaveTradesAsync(List<Trade> trades)
        {
            foreach (var item in trades)
                await SaveTradeAsync(item);
        }
    }
}