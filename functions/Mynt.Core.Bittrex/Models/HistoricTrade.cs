using System;

namespace Mynt.Core.Bittrex.Models
{
    public class HistoricTrade
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double Total { get; set; }
        public string FillType { get; set; }
        public string OrderType { get; set; }
    }
}
