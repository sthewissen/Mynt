using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Mynt.Core.Backtester;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Data.MongoDB
{
    public class MongoDBDataStoreBacktest : IDataStoreBacktest
    {
        private MongoClient client;
        private IMongoDatabase database;
        public static MongoDBOptions mongoDbOptions;
        public string mongoDbBaseName;

        public MongoDBDataStoreBacktest(MongoDBOptions options)
        {
            mongoDbOptions = options;
            client = new MongoClient(options.MongoUrl);
            database = client.GetDatabase(options.MongoDatabaseName);
            mongoDbBaseName = "Backtest_Candle_";
        }

        public static string GetDatabase(BacktestOptions backtestOptions)
        {
            return backtestOptions.Exchange + "_" + backtestOptions.Coin + "_" + backtestOptions.CandlePeriod;
        }

        public class DataStoreBacktest
        {
            private MongoClient client;
            private IMongoDatabase database;
            private static Dictionary<string, DataStoreBacktest> instance = new Dictionary<string, DataStoreBacktest>();

            private DataStoreBacktest(string databaseName)
            {
                client = new MongoClient(mongoDbOptions.MongoUrl);
                database = client.GetDatabase(databaseName);
            }

            public static DataStoreBacktest GetInstance(string databaseName)
            {
                if (!instance.ContainsKey(databaseName))
                {
                    instance["databaseName"] = new DataStoreBacktest(databaseName);
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

        public async Task<List<Candle>> GetBacktestCandlesBetweenTime(BacktestOptions backtestOptions)
        {
            IMongoCollection<CandleAdapter> candleCollection = DataStoreBacktest.GetInstance(mongoDbBaseName + backtestOptions.CandlePeriod).GetTable<CandleAdapter>(backtestOptions.Exchange + "_" + backtestOptions.Coin);
            List<CandleAdapter> candles = await candleCollection.Find(entry => entry.Timestamp >= backtestOptions.StartDate && entry.Timestamp <= backtestOptions.EndDate).ToListAsync();
            var items = Mapping.Mapper.Map<List<Candle>>(candles);
            return items;
        }

        public async Task<Candle> GetBacktestFirstCandle(BacktestOptions backtestOptions)
        {
            IMongoCollection<CandleAdapter> candleCollection = DataStoreBacktest.GetInstance(mongoDbBaseName + backtestOptions.CandlePeriod).GetTable<CandleAdapter>(backtestOptions.Exchange + "_" + backtestOptions.Coin);
            CandleAdapter lastCandle = await candleCollection.Find(_ => true).SortBy(e => e.Timestamp).Limit(1).FirstOrDefaultAsync();
            var items = Mapping.Mapper.Map<Candle>(lastCandle);
            return items;
        }

        public async Task<Candle> GetBacktestLastCandle(BacktestOptions backtestOptions)
        {
            IMongoCollection<CandleAdapter> candleCollection = DataStoreBacktest.GetInstance(mongoDbBaseName + backtestOptions.CandlePeriod).GetTable<CandleAdapter>(backtestOptions.Exchange + "_" + backtestOptions.Coin);
            CandleAdapter lastCandle = await candleCollection.Find(_ => true).SortByDescending(e => e.Timestamp).Limit(1).FirstOrDefaultAsync();
            var items = Mapping.Mapper.Map<Candle>(lastCandle);
            return items;
        }

        public async Task SaveBacktestCandlesBulk(List<Candle> candles, BacktestOptions backtestOptions)
        {
            var items = Mapping.Mapper.Map<List<CandleAdapter>>(candles);
            IMongoCollection<CandleAdapter> candleCollection = DataStoreBacktest.GetInstance(mongoDbBaseName + backtestOptions.CandlePeriod).GetTable<CandleAdapter>(backtestOptions.Exchange + "_" + backtestOptions.Coin);
            await candleCollection.InsertManyAsync(items);
        }

        public async Task SaveBacktestCandlesBulkCheckExisting(List<Candle> candles, BacktestOptions backtestOptions)
        {
            var items = Mapping.Mapper.Map<List<CandleAdapter>>(candles);
            IMongoCollection<CandleAdapter> candleCollection = DataStoreBacktest.GetInstance(mongoDbBaseName + backtestOptions.CandlePeriod).GetTable<CandleAdapter>(backtestOptions.Exchange + "_" + backtestOptions.Coin);
            FindOptions<CandleAdapter> marketCandleFindOptions = new FindOptions<CandleAdapter> { Limit = 1 };
            foreach (var item in items)
            {
                IAsyncCursor<CandleAdapter> checkData = await candleCollection.FindAsync(x => x.Timestamp.Equals(item.Timestamp), marketCandleFindOptions);
                if (await checkData.FirstOrDefaultAsync() == null)
                {
                    await candleCollection.InsertOneAsync(item);
                }
            }
        }

        public async Task SaveBacktestCandle(Candle candle, BacktestOptions backtestOptions)
        {
            var item = Mapping.Mapper.Map<CandleAdapter>(candle);
            IMongoCollection<CandleAdapter> candleCollection = DataStoreBacktest.GetInstance(mongoDbBaseName + backtestOptions.CandlePeriod).GetTable<CandleAdapter>(backtestOptions.Exchange + "_" + backtestOptions.Coin);
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
