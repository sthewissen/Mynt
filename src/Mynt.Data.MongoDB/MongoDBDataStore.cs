using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Mynt.Core.Backtester;
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
			database = client.GetDatabase(options.MongoDatabaseName);
            ordersAdapter = database.GetCollection<TradeAdapter>("Orders");
            traderAdapter = database.GetCollection<TraderAdapter>("Traders");
        }

        public static string GetDatabase(BacktestOptions backtestOptions)
        {
            return backtestOptions.Exchange + "_" + backtestOptions.Coin + "_" + backtestOptions.CandlePeriod;
        }

        public class DataStore
        {
            private MongoClient client;
            private IMongoDatabase database;
            private static Dictionary<string, DataStore> instance = new Dictionary<string, DataStore>();

            private DataStore(string databaseName)
            {
                client = new MongoClient(mongoDbOptions.MongoUrl);
                database = client.GetDatabase(databaseName);
            }

            public static DataStore GetInstance(string databaseName)
            {
                if (!instance.ContainsKey(databaseName))
                {
                    instance["databaseName"] = new DataStore(databaseName);
                }
                return instance["databaseName"];
            }

            public IMongoCollection<T> GetTable<T>(string collectionName = null) where T : new()
            {
                if (collectionName == null)
                {
                    return database.GetCollection<T>(typeof(T).Name);
                }
                return database.GetCollection<T>(collectionName);
            }
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


        /* Backtester */

        public async Task<List<Candle>> GetBacktestCandlesBetweenTime(BacktestOptions backtestOptions)
        {
            IMongoCollection<CandleAdapter> candleCollection = DataStore.GetInstance("Backtest_Candle_" + backtestOptions.CandlePeriod).GetTable<CandleAdapter>(backtestOptions.Exchange + "_" + backtestOptions.Coin);
            List<CandleAdapter> candles = await candleCollection.Find(entry => entry.Timestamp >= backtestOptions.StartDate && entry.Timestamp <= backtestOptions.EndDate).ToListAsync();
            var items = Mapping.Mapper.Map<List<Candle>>(candles);
            return items;
        }

        public async Task<Candle> GetBacktestFirstCandle(BacktestOptions backtestOptions)
        {
            IMongoCollection<CandleAdapter> candleCollection = DataStore.GetInstance("Backtest_Candle_" + backtestOptions.CandlePeriod).GetTable<CandleAdapter>(backtestOptions.Exchange + "_" + backtestOptions.Coin);
            CandleAdapter lastCandle = await candleCollection.Find(_ => true).SortBy(e => e.Timestamp).Limit(1).FirstOrDefaultAsync();
            var items = Mapping.Mapper.Map<Candle>(lastCandle);
            return items;
        }

        public async Task<Candle> GetBacktestLastCandle(BacktestOptions backtestOptions)
        {
            IMongoCollection<CandleAdapter> candleCollection = DataStore.GetInstance("Backtest_Candle_" + backtestOptions.CandlePeriod).GetTable<CandleAdapter>(backtestOptions.Exchange + "_" + backtestOptions.Coin);
            CandleAdapter lastCandle = await candleCollection.Find(_ => true).SortByDescending(e => e.Timestamp).Limit(1).FirstOrDefaultAsync();
            var items = Mapping.Mapper.Map<Candle>(lastCandle);
            return items;
        }

        public async Task SaveBacktestCandlesBulk(List<Candle> candles, BacktestOptions backtestOptions)
        {
            var items = Mapping.Mapper.Map<List<CandleAdapter>>(candles);
            IMongoCollection<CandleAdapter> candleCollection = DataStore.GetInstance("Backtest_Candle_" + backtestOptions.CandlePeriod).GetTable<CandleAdapter>(backtestOptions.Exchange + "_" + backtestOptions.Coin);
            await candleCollection.InsertManyAsync(items);
        }

        public async Task SaveBacktestCandle(Candle candle, BacktestOptions backtestOptions)
        {
            var item = Mapping.Mapper.Map<CandleAdapter>(candle);
            IMongoCollection<CandleAdapter> candleCollection = DataStore.GetInstance("Backtest_Candle_" + backtestOptions.CandlePeriod).GetTable<CandleAdapter>(backtestOptions.Exchange + "_" + backtestOptions.Coin);
            FindOptions<CandleAdapter> marketCandleFindOptions = new FindOptions<CandleAdapter> { Limit = 1 };
            IAsyncCursor<CandleAdapter> checkData = await candleCollection.FindAsync(x => x.Timestamp == item.Timestamp, marketCandleFindOptions);
            if (await checkData.FirstOrDefaultAsync() == null)
            {
                await candleCollection.InsertOneAsync(item);
            }
        }

        public async Task<List<string>> GetBacktestAllDatabases(BacktestOptions backtestOptions)
        {
            List<string> allDatabases = new List<string>();
            var dbList = await client.GetDatabase(mongoDbOptions.MongoDatabaseName).ListCollectionsAsync();
            foreach (var item in await dbList.ToListAsync())
            {
                allDatabases.Add(item.ToString());
            }
            return allDatabases;
        }

        public async Task DeleteBacktestDatabase(BacktestOptions backtestOptions)
        {
            var dbList = client.GetDatabase(mongoDbOptions.MongoDatabaseName);
            dbList.DropCollection(backtestOptions.Exchange + "_" + backtestOptions.Coin);
        }

    }
}
