using System;

namespace Mynt.Core.Bittrex.Models
{
    /// <summary>
    /// The result of the /account/getdepositaddress/ end point
    /// </summary>
    public class DepositAddress
    {
        /// <summary>
        /// The currency of the deposit address, i.e. BTC
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// The wallet address
        /// </summary>
        public string Address { get; set; }
    }
}
