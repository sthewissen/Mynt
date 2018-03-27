using System;

namespace Mynt.Core.Models
{
    public class Trader
    {
        public decimal StakeAmount { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsBusy { get; set; }
    }
}
