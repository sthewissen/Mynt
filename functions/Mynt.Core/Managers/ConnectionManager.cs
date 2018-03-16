﻿using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mynt.Core.Managers
{
    public static class ConnectionManager
    {
        public static async Task<CloudTable> GetTableConnection(string connectionString, string tableName, bool dryRun)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName + (dryRun ? "DryRun" : ""));
            await table.CreateIfNotExistsAsync();

            return table;
        }
    }
}
