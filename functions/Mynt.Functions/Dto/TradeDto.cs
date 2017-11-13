using System;
using Mynt.Core.Models;

namespace Mynt.Functions.Dto
{
    public class TradeDto
    {
        public string Market { get; set; }
        public double OpenRate { get; set; }
        public double? CloseRate { get; set; }
        public double? CloseProfit { get; set; }
        public double? CloseProfitPercentage { get; set; }

        public double StakeAmount { get; set; }
        public double Quantity { get; set; }

        public bool IsOpen { get; set; }

        public DateTime OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        
        public string Uuid { get; set; }

        public double CurrentRate { get; set; }
    }
}
