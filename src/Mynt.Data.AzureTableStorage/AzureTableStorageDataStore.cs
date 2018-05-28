using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Mynt.Core.Backtester;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Data.AzureTableStorage
{
    public class AzureTableStorageDataStore : IDataStore
    {
        private readonly AzureTableStorageOptions _options;
        private CloudTable _orderTable;
        private CloudTable _traderTable;

        public AzureTableStorageDataStore(AzureTableStorageOptions options)
        {
            _options = options;
        }

        public async Task InitializeAsync()
        {
            // First initialize a few things
            _orderTable = await GetTableConnection(_options.AzureTableStorageConnectionString, "orders");
            _traderTable = await GetTableConnection(_options.AzureTableStorageConnectionString, "traders");
        }

        private static async Task<CloudTable> GetTableConnection(string connectionString, string tableName)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            return table;
        }

        public async Task<List<Trader>> GetTradersAsync()
        {
            var query = new TableQuery<TraderAdapter>().Where(TableQuery.GenerateFilterConditionForBool("IsArchived", QueryComparisons.Equal, false));

            TableContinuationToken token = null;
            var items = new List<TraderAdapter>();
            do
            {
                var results = await _traderTable.ExecuteQuerySegmentedAsync(query, token);
                items.AddRange(results);
                token = results.ContinuationToken;
            } while (token != null);

            var destination = Mapping.Mapper.Map<List<Trader>>(items);
            return destination;
        }

        public async Task<List<Trade>> GetActiveTradesAsync()
        {
            var query = new TableQuery<TradeAdapter>().Where(TableQuery.GenerateFilterConditionForBool("IsOpen", QueryComparisons.Equal, true));
            TableContinuationToken token = null;
            var items = new List<TradeAdapter>();
            do
            {
                var results = await _orderTable.ExecuteQuerySegmentedAsync(query, token);
                items.AddRange(results);
                token = results.ContinuationToken;
            } while (token != null);

            var destination = Mapping.Mapper.Map<List<Trade>>(items);
            return destination;
        }

        public async Task<List<Trader>> GetAvailableTradersAsync()
        {
            var filter1 = TableQuery.GenerateFilterConditionForBool("IsBusy", QueryComparisons.Equal, false);
            var filter2 = TableQuery.GenerateFilterConditionForBool("IsArchived", QueryComparisons.Equal, false);
            var filter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);

            var query = new TableQuery<TraderAdapter>().Where(filter);
            TableContinuationToken token = null;
            var items = new List<TraderAdapter>();
            do
            {
                var results = await _traderTable.ExecuteQuerySegmentedAsync(query, token);
                items.AddRange(results);
                token = results.ContinuationToken;
            } while (token != null);

            var destination = Mapping.Mapper.Map<List<Trader>>(items);
            return destination;
        }

        public async Task<List<Trader>> GetBusyTradersAsync()
        {
            var filter1 = TableQuery.GenerateFilterConditionForBool("IsBusy", QueryComparisons.Equal, true);
            var filter2 = TableQuery.GenerateFilterConditionForBool("IsArchived", QueryComparisons.Equal, false);
            var filter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);

            var query = new TableQuery<TraderAdapter>().Where(filter);
            TableContinuationToken token = null;
            var items = new List<TraderAdapter>();
            do
            {
                var results = await _traderTable.ExecuteQuerySegmentedAsync(query, token);
                items.AddRange(results);
                token = results.ContinuationToken;
            } while (token != null);

            var destination = Mapping.Mapper.Map<List<Trader>>(items);
            return destination;
        }

        public async Task SaveTradeAsync(Trade trade)
        {
            var orderBatch = new TableBatchOperation();
            var item = Mapping.Mapper.Map<TradeAdapter>(trade);
            orderBatch.Add(TableOperation.InsertOrReplace(item));
            await _orderTable.ExecuteBatchAsync(orderBatch);
        }

        public async Task SaveTraderAsync(Trader trader)
        {
            var traderBatch = new TableBatchOperation();
            var item = Mapping.Mapper.Map<TraderAdapter>(trader);
            traderBatch.Add(TableOperation.InsertOrReplace(item));
            await _traderTable.ExecuteBatchAsync(traderBatch);
        }

        public async Task SaveTradersAsync(List<Trader> traders)
        {
            var traderBatch = new TableBatchOperation();
            var items = Mapping.Mapper.Map<List<TraderAdapter>>(traders);

            foreach(var item in items)
            {
                traderBatch.Add(TableOperation.InsertOrReplace(item));
            }

            if (traderBatch.Count > 0)
                await _traderTable.ExecuteBatchAsync(traderBatch);
        }

        public async Task SaveTradesAsync(List<Trade> trades)
        {
            var tradeBatch = new TableBatchOperation();
            var items = Mapping.Mapper.Map<List<TradeAdapter>>(trades);

            foreach (var item in items)
            {
                tradeBatch.Add(TableOperation.InsertOrReplace(item));
            }

            if (tradeBatch.Count > 0)
                await _orderTable.ExecuteBatchAsync(tradeBatch);
        }

    }
}
