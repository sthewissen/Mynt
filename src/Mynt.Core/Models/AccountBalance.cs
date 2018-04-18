using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Models
{
    public class AccountBalance
    {
        private readonly string currency;

        private readonly decimal available;

        private readonly decimal pending;

        public AccountBalance(string currency, decimal available, decimal pending)
        {
            this.currency = currency;
            this.available = available;
            this.pending = pending;
        }

        public string Currency => currency;

        public decimal Balance => available + pending;

        public decimal Available => available;

        public decimal Pending => pending;
    }
}
