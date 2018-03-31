using System;
using Mynt.Core.Configuration;

namespace Mynt.Data.AzureTableStorage
{
    public class AzureTableStorageOptions : BaseSettings
    {
        public string ConnectionString { get; set; }

        public AzureTableStorageOptions()
        { 
            TrySetFromConfig(() => ConnectionString = AppSettings.Get<string>("AzureTableStorage" + nameof(ConnectionString)));
       }
    }
}
