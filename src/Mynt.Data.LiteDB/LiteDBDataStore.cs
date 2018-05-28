using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LiteDB;
using Mynt.Core.Backtester;
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
            GetDatabase(new BacktestOptions());
        }

        public static string GetDatabase(BacktestOptions backtestOptions)
        {
            if (!Directory.Exists(backtestOptions.DataFolder))
                Directory.CreateDirectory(backtestOptions.DataFolder);

            if (backtestOptions.Coin == null)
            {
                return backtestOptions.DataFolder.Replace("\\", "/");
            }
            return backtestOptions.DataFolder.Replace("\\", "/") + "/" + backtestOptions.Exchange + "_" + backtestOptions.Coin + ".db";
        }

        private static readonly Dictionary<string, DataStore> DatabaseInstances = new Dictionary<string, DataStore>();

        private class DataStore
        {
            private readonly LiteDatabase _liteDatabase;

            private DataStore(string databasePath)
            {
                // Workaround on OSX -> Dont support Locking/unlocking file regions 
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    _liteDatabase = new LiteDatabase("filename=" + databasePath + ";mode=Exclusive;utc=true");
                }
                else
                {
                    _liteDatabase = new LiteDatabase("filename=" + databasePath + ";mode=Exclusive;mode=Shared;utc=true");
                }
            }

            public static DataStore GetInstance(string databasePath)
            {
                if (!DatabaseInstances.ContainsKey(databasePath))
                {
                    DatabaseInstances[databasePath] = new DataStore(databasePath);
                }
                return DatabaseInstances[databasePath];
            }

            public LiteCollection<T> GetTable<T>(string collectionName = null) where T : new()
            {
                if (collectionName == null)
                {
                    return _liteDatabase.GetCollection<T>(typeof(T).Name);
                }
                return _liteDatabase.GetCollection<T>(collectionName);
            }
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

        /* Backtester */

        public async Task<List<Candle>> GetBacktestCandlesBetweenTime(BacktestOptions backtestOptions)
        {
            LiteCollection<CandleAdapter> candleCollection = DataStore.GetInstance(GetDatabase(backtestOptions)).GetTable<CandleAdapter>("Candle_" + backtestOptions.CandlePeriod);
            candleCollection.EnsureIndex("Timestamp");
            List<CandleAdapter> candles = candleCollection.Find(Query.Between("Timestamp", backtestOptions.StartDate, backtestOptions.EndDate), Query.Ascending).ToList();
            var items = Mapping.Mapper.Map<List<Candle>>(candles);
            return items;
        }

        public async Task<Candle> GetBacktestFirstCandle(BacktestOptions backtestOptions)
        {
            LiteCollection<CandleAdapter> candleCollection = DataStore.GetInstance(GetDatabase(backtestOptions)).GetTable<CandleAdapter>("Candle_" + backtestOptions.CandlePeriod);
            candleCollection.EnsureIndex("Timestamp");
            CandleAdapter lastCandle = candleCollection.Find(Query.All("Timestamp"), limit: 1).FirstOrDefault();
            var items = Mapping.Mapper.Map<Candle>(lastCandle);
            return items;
        }

        public async Task<Candle> GetBacktestLastCandle(BacktestOptions backtestOptions)
        {
            LiteCollection<CandleAdapter> candleCollection = DataStore.GetInstance(GetDatabase(backtestOptions)).GetTable<CandleAdapter>("Candle_" + backtestOptions.CandlePeriod);
            candleCollection.EnsureIndex("Timestamp");
            CandleAdapter lastCandle = candleCollection.Find(Query.All("Timestamp", Query.Descending), limit: 1).FirstOrDefault();
            var items = Mapping.Mapper.Map<Candle>(lastCandle);
            return items;
        }

        public async Task SaveBacktestCandlesBulk(List<Candle> candles, BacktestOptions backtestOptions)
        {
            var items = Mapping.Mapper.Map<List<CandleAdapter>>(candles);
            LiteCollection<CandleAdapter> candleCollection = DataStore.GetInstance(GetDatabase(backtestOptions)).GetTable<CandleAdapter>("Candle_" + backtestOptions.CandlePeriod);
            candleCollection.EnsureIndex("Timestamp");
            candleCollection.InsertBulk(items);
        }

        public async Task SaveBacktestCandlesBulkCheckExisting(List<Candle> candles, BacktestOptions backtestOptions)
        {
            var items = Mapping.Mapper.Map<List<CandleAdapter>>(candles);
            LiteCollection<CandleAdapter> candleCollection = DataStore.GetInstance(GetDatabase(backtestOptions)).GetTable<CandleAdapter>("Candle_" + backtestOptions.CandlePeriod); foreach (var item in items)
            {
                var checkData = candleCollection.FindOne(x => x.Timestamp == item.Timestamp);
                if (checkData == null)
                {
                    candleCollection.Insert(item);
                }
            }
        }

        public async Task SaveBacktestCandle(Candle candle, BacktestOptions backtestOptions)
        {
            var item = Mapping.Mapper.Map<CandleAdapter>(candle);
            LiteCollection<CandleAdapter> candleCollection = DataStore.GetInstance(GetDatabase(backtestOptions)).GetTable<CandleAdapter>("Candle_" + backtestOptions.CandlePeriod);
            candleCollection.EnsureIndex("Timestamp");
            var newCandle = candleCollection.FindOne(x => x.Timestamp == item.Timestamp);
            if (newCandle == null)
            {
                candleCollection.Insert(item);
            }
        }

        public async Task<List<string>> GetBacktestAllDatabases(BacktestOptions backtestOptions)
        {
            List<string> allDatabaseFiles = Directory.GetFiles(backtestOptions.DataFolder, "*.db", SearchOption.TopDirectoryOnly).ToList();
            return allDatabaseFiles;
        }

        public async Task DeleteBacktestDatabase(BacktestOptions backtestOptions)
        {
            if (File.Exists(GetDatabase(backtestOptions)))
            {
                File.Delete(GetDatabase(backtestOptions));
            }
        }

    }
}
