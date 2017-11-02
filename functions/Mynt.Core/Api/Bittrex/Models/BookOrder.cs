using System;

namespace Mynt.Core.Api.Bittrex.Models
{
    public enum BookOrderType
    {
        Buy,
        Sell
    }

    /// <summary>
    /// The result of the /public/getorderbook end point
    /// This contains a single book order for the request
    /// </summary>
    public class BookOrder
    {
        public BookOrderType OrderType { get; set; }
        public double Quantity { get; set; }
        public double Rate { get; set; }
    }
}
