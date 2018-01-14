using System;

namespace Mynt.Core.Bittrex.Models
{
    /// <summary>
    /// Contains the account balance for a particular currency
    /// </summary>
    public class AccountBalance
    {
        public string Currency { get; set; }
        public double Balance { get; set; }
        public double Available { get; set; }
        public double Pending { get; set; }
        public string CryptoAddress { get; set; }
        public bool Requested { get; set; }
        public string Uuid { get; set; }
    }
}
