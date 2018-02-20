using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mynt.Core.Models
{
    public class Trader : TableEntity
    {
        public double StakeAmount { get; set; }
        public double CurrentBalance { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsBusy { get; set; }
    }
}
