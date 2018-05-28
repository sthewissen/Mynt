using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Data.MongoDB
{
    public class MongoDBDataStore : IDataStore
    {
        private MongoClient client;
        private IMongoDatabase database;
        public static MongoDBOptions mongoDbOptions;
        private IMongoCollection<TraderAdapter> traderAdapter;
        private IMongoCollection<TradeAdapter> ordersAdapter;

        public MongoDBDataStore(MongoDBOptions options)
        {
            mongoDbOptions = options;
            client = new MongoClient(options.MongoUrl);
            ordersAdapter = database.GetCollection<TradeAdapter>("Orders");
            traderAdapter = database.GetCollection<TraderAdapter>("Traders");
        }

        public async Task InitializeAsync()
        {
        }

        public async Task<List<Trade>> GetActiveTradesAsync()
        {
            var trades = await ordersAdapter.Find(x => x.IsOpen).ToListAsync();
            var items = Mapping.Mapper.Map<List<Trade>>(trades);

            return items;
        }

        public async Task<List<Trader>> GetAvailableTradersAsync()
        {
            var traders = await traderAdapter.Find(x => !x.IsBusy && !x.IsArchived).ToListAsync();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return items;
        }

        public async Task<List<Trader>> GetBusyTradersAsync()
        {
            var traders = await traderAdapter.Find(x => x.IsBusy && !x.IsArchived).ToListAsync();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return items;
        }

        public async Task SaveTradeAsync(Trade trade)
        {
            var item = Mapping.Mapper.Map<TradeAdapter>(trade);
            TradeAdapter checkExist = await ordersAdapter.Find(x => x.TradeId.Equals(item.TradeId)).FirstOrDefaultAsync();
            if (checkExist != null)
            {
                await ordersAdapter.ReplaceOneAsync(x => x.TradeId.Equals(item.TradeId), item);
            } else
            {
                await ordersAdapter.InsertOneAsync(item);
            }
        }

        public async Task SaveTraderAsync(Trader trader)
        {
            var item = Mapping.Mapper.Map<TraderAdapter>(trader);
            TraderAdapter checkExist = await traderAdapter.Find(x => x.Identifier.Equals(item.Identifier)).FirstOrDefaultAsync();
            if (checkExist != null)
            {
                await traderAdapter.ReplaceOneAsync(x => x.Identifier.Equals(item.Identifier), item);
            }
            else
            {
                await traderAdapter.InsertOneAsync(item);
            }
        }

        public async Task SaveTradersAsync(List<Trader> traders)
        {
            var items = Mapping.Mapper.Map<List<TraderAdapter>>(traders);

            foreach (var item in items)
            {
                TraderAdapter checkExist = await traderAdapter.Find(x => x.Identifier.Equals(item.Identifier)).FirstOrDefaultAsync();
                if (checkExist != null)
                {
                    await traderAdapter.ReplaceOneAsync(x => x.Identifier.Equals(item.Identifier), item);
                }
                else
                {
                    await traderAdapter.InsertOneAsync(item);
                }
            }
        }

        public async Task SaveTradesAsync(List<Trade> trades)
        {
            var items = Mapping.Mapper.Map<List<TradeAdapter>>(trades);

            foreach (var item in items)
            {
                TradeAdapter checkExist = await ordersAdapter.Find(x => x.TradeId.Equals(item.TradeId)).FirstOrDefaultAsync();
                if (checkExist != null)
                {
                    await ordersAdapter.ReplaceOneAsync(x => x.TradeId.Equals(item.TradeId), item);
                }
                else
                {
                    await ordersAdapter.InsertOneAsync(item);
                }
            }
        }

        public async Task<List<Trader>> GetTradersAsync()
        {
            var traders = await traderAdapter.Find(_ => true).ToListAsync();
            var items = Mapping.Mapper.Map<List<Trader>>(traders);

            return items;
        }

    }
}
