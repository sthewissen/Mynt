using System;
using Mynt.Core.Enums;

namespace Mynt.Core.Models
{
    public class OpenOrder
    {
        public string OrderId { get; set; }
        public string Exchange { get; set; }
        public string Market { get; set; }
        public decimal OriginalQuantity { get; set; }
        public decimal ExecutedQuantity { get; set; }
        public decimal Price { get; set; }
        public OrderStatus Status { get; set; }
        public OrderSide Side { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
