using System;
using Mynt.Core.Enums;

namespace Mynt.Core.Models
{
    public class Trade
    {
        // Used as primary key for the different data storage mechanisms.
        public int Id { get; set; }

        public string TradeId { get; set; }
        public string TraderId { get; set; }
        public string Market { get; set; }

        public decimal OpenRate { get; set; }
        public decimal? CloseRate { get; set; }
        public decimal? CloseProfit { get; set; }
        public decimal? CloseProfitPercentage { get; set; }

        public decimal StakeAmount { get; set; }
        public decimal Quantity { get; set; }

        public bool IsOpen { get; set; }
        public bool IsBuying { get; set; }
        public bool IsSelling { get; set; }

        public string OpenOrderId { get; set; }
        public string BuyOrderId { get; set; }
        public string SellOrderId { get; set; }

        public DateTime OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }

        public string StrategyUsed { get; set; }
        public decimal? StopLossRate { get; set; }
        public SellType SellType { get; set; }

        public Trade()
        {
            TradeId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            IsOpen = true;
            OpenDate = DateTime.UtcNow;
        }

        // Used for Mynt.AspNetCore.Host UI output
        public decimal? OpenProfit { get; set; }
        public decimal? OpenProfitPercentage { get; set; }
    }
}
