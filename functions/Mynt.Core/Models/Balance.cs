using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mynt.Core.Models
{
    public class Balance : TableEntity
    {
        public double TotalBalance { get; set; }
        public double Profit { get; set; }
        public DateTime? BalanceDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
