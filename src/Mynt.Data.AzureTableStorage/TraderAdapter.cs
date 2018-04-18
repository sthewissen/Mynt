using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mynt.Data.AzureTableStorage
{
    internal class TraderAdapter : TableEntity
    {
        public string Identifier { get; set; }
        public double StakeAmount { get; set; }
        public double CurrentBalance { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsBusy { get; set; }
        public bool IsArchived { get; set; }
    }
}
