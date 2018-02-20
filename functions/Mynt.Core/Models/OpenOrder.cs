using System;

namespace Mynt.Core.Models
{
    /// <summary>
    /// An open order result from the /market/getopenorders end point
    /// </summary>
    public class OpenOrder
    {
        public string OrderUuid { get; set; }
        public string Exchange { get; set; }
        public string OrderType { get; set; }
        public double Quantity { get; set; }
        public double QuantityRemaining { get; set; }
        public double Limit { get; set; }
        public double Price { get; set; }
        public double? PricePerUnit { get; set; }
        public bool CancelInitiated { get; set; }
        public bool ImmediateOrCancel { get; set; }
    }
}
