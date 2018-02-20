using System;

namespace Mynt.Core.Bittrex.Models
{
    /// <summary>
    /// The result of the /public/getcurrencies end point
    /// This contains the details of a tradeable currency on the bittrex trading platform
    /// </summary>
    public class CryptoCurrency
    {
        public string Currency { get; set; }
        public string CurrencyLong { get; set; }
        public int MinConfirmation { get; set; }
        public double TxFee { get; set; }
        public bool IsActive { get; set; }
        public string CoinType { get; set; }
        public string BaseAddress { get; set; }
        public string Notice { get; set; }
    }
}
