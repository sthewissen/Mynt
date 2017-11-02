using System;

namespace Mynt.Core.Api.Bittrex.Models
{
    /// <summary>
    /// The result of the /public/getmarkets end point
    /// This contains the details of a tradeable market
    /// </summary>
    public class Market
    {
        public string MarketCurrency { get; set; }
        public string MarketCurrencyLong { get; set; }
        public string BaseCurrency { get; set; }
        public string BaseCurrencyLong { get; set; }
        public double MinTradeSize { get; set; }
        public string MarketName { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
    }
}
