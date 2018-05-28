using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Data.LiteDB
{

    public class LiteDBDataStore : IDataStore
    {
        private LiteDatabase database;
        private LiteCollection<TraderAdapter> traderAdapter;
        private LiteCollection<TradeAdapter> ordersAdapter;

        public LiteDBDataStore(LiteDBOptions options)
        {
            database = new LiteDatabase(options.LiteDBName);
            ordersAdapter = database.GetCollection<TradeAdapter>("Orders");
            traderAdapter = database.GetCollection<TraderAdapter>("Traders");
        }

        public async Task InitializeAsync()
        {
        }

        public async Task<List<Trade>> GetActiveTradesAsync()
        {
            var trades = ordersAdapter.Find(x => x.IsOpen).ToList();
            var items = Mapping.Mapper.Map<List<Trade>>(trades);

            return items;
        }

        public async Task<List<Trader>> GetAvailableTradersAsync()
        {
            var traders = traderAdapter.Find(x => !x.IsBusy && !x.IsArchived).ToList();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return items;
        }

        public async Task<List<Trader>> GetBusyTradersAsync()
        {
            var traders = traderAdapter.Find(x => x.IsBusy && !x.IsArchived).ToList();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return items;
        }

        public async Task SaveTradeAsync(Trade trade)
        {
            var item = Mapping.Mapper.Map<TradeAdapter>(trade);
            TradeAdapter checkExist = ordersAdapter.Find(x => x.TradeId.Equals(item.TradeId)).FirstOrDefault();
            ordersAdapter.Upsert(item);
        }

        public async Task SaveTraderAsync(Trader trader)
        {
            var item = Mapping.Mapper.Map<TraderAdapter>(trader);
            TraderAdapter checkExist = traderAdapter.Find(x => x.Identifier.Equals(item.Identifier)).FirstOrDefault();
            traderAdapter.Upsert(item);
        }

        public async Task SaveTradersAsync(List<Trader> traders)
        {
            var items = Mapping.Mapper.Map<List<TraderAdapter>>(traders);

            foreach (var item in items)
            {
                TraderAdapter checkExist = traderAdapter.Find(x => x.Identifier.Equals(item.Identifier)).FirstOrDefault();
                traderAdapter.Upsert(item);
            }
        }

        public async Task SaveTradesAsync(List<Trade> trades)
        {
            var items = Mapping.Mapper.Map<List<TradeAdapter>>(trades);

            foreach (var item in items)
            {
                TradeAdapter checkExist = ordersAdapter.Find(x => x.TradeId.Equals(item.TradeId)).FirstOrDefault();
                ordersAdapter.Upsert(item);
            }
        }

        public async Task<List<Trader>> GetTradersAsync()
        {
            var traders = traderAdapter.FindAll().ToList();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return items;
        }
    }
}
