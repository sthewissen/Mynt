using System;

namespace Mynt.Core.Models
{
    public class Trader
    {
        public string Identifier { get; set; }
        public decimal StakeAmount { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsBusy { get; set; }
        public bool IsArchived { get; set; }
    }
}
