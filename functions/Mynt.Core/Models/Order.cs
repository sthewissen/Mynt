using System;
using Mynt.Core.Enums;

namespace Mynt.Core.Models
{
    public class Order
    {
        public string Symbol { get; set; }

        public string OrderId { get; set; }

        public double Price { get; set; }

        public double OriginalQuantity { get; set; }

        public double ExecutedQuantity { get; set; }

        public OrderStatus Status { get; set; }

        public TimeInForce TimeInForce { get; set; }

        public OrderType Type { get; set; }

        public OrderSide Side { get; set; }

        public double StopPrice { get; set; }

        public DateTime Time { get; set; }
    }
}
