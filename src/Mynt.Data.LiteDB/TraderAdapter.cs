using System;

namespace Mynt.Data.LiteDB
{
    public class TraderAdapter
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
        public double StakeAmount { get; set; }
        public double CurrentBalance { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsBusy { get; set; }
        public bool IsArchived { get; set; }
    }
}
