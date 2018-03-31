using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mynt.Core.Enums;

namespace Mynt.Data.SqlServer
{
    public class TradeAdapter
    {
        [Key]
        public int TradeId { get; set; }

        public string TraderId { get; set; }
        public string Market { get; set; }

        public double OpenRate { get; set; }
        public double? CloseRate { get; set; }
        public double? CloseProfit { get; set; }
        public double? CloseProfitPercentage { get; set; }

        public double StakeAmount { get; set; }
        public double Quantity { get; set; }

        public bool IsOpen { get; set; }
        public bool IsBuying { get; set; }
        public bool IsSelling { get; set; }

        public string OpenOrderId { get; set; }
        public string BuyOrderId { get; set; }
        public string SellOrderId { get; set; }

        public DateTime OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }

        public string StrategyUsed { get; set; }
        public double? StopLossRate { get; set; }
        public SellType SellType { get; set; }
    }
}
