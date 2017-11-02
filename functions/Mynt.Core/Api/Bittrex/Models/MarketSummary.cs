using System;

namespace Mynt.Core.Api.Bittrex.Models
{
    /// <summary>    
    /// The result of the /public/getmarketsummaries end point
    /// This contains a summary of the last 24 hours trading for the market
    /// </summary>
    public class MarketSummary
    {
        public string MarketName { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Volume { get; set; }
        public double Last { get; set; }
        public double BaseVolume { get; set; }
        public DateTime TimeStamp { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public int OpenBuyOrders { get; set; }
        public int OpenSellOrders { get; set; }
        public double PrevDay { get; set; }
        public DateTime Created { get; set; }
    }
}
