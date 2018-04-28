using System;
using System.Collections.Generic;

namespace Mynt.Core.Models
{
    public class OrderBook
    {
        public List<OrderBookEntry> Asks { get; set; }
        public List<OrderBookEntry> Bids { get; set; }
    }
}
