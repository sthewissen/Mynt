using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mynt.Core.Models
{
    public class Trade : TableEntity
    {
        public string TraderId { get; set; }

        public string Market { get; set; }

        public double OpenRate { get; set; }
        public double? CloseRate { get; set; }
        public double? CloseProfit { get; set; }
        public double? CloseProfitPercentage { get; set; }

        public double StakeAmount { get; set; }
        public double Quantity { get; set; }

        public bool IsOpen { get; set; }

        public string OpenOrderId { get; set; }
        public string BuyOrderId { get; set; }
        public string SellOrderId { get; set; }

        public DateTime OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }

        public string StrategyUsed { get; set; }
        public double? StopLossAnchor { get; set; }
        public SellType SellType { get; set; }

        public Trade()
        {
            IsOpen = true;
            OpenDate = DateTime.UtcNow;
        }
    }
}
