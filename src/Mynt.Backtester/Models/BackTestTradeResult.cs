using System;
namespace Mynt.Backtester.Models
{
    public class BackTestTradeResult
    {
        public decimal OpenRate { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitPercentage { get; set; }
        public int Duration { get; set; }
		public decimal CloseRate { get; set; }
		public decimal Quatity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
