using System;

namespace Mynt.Core.Bittrex.Models
{
    /// <summary>
    /// The result of the /public/getticker
    /// </summary>
    public class Ticker
    {
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Last { get; set; }
    }
}
