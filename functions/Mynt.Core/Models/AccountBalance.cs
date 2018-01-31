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

        private readonly double available;

        private readonly double pending;

        public AccountBalance(string currency, double available, double pending)
        {
            this.currency = currency;
            this.available = available;
            this.pending = pending;
        }

        public string Currency => currency;

        public double Balance => available + pending;

        public double Available => available;

        public double Pending => pending;
    }
}
