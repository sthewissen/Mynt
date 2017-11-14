using System;
namespace Mynt.Models
{
    public class Trade
    {
        public string Market { get; set; }
        public string Currency
        {
            get
            {
                if (Market != null)
                    return Market.Replace("BTC-", "");

                return string.Empty;
            }
        }

        public double OpenRate { get; set; }
        public double? CloseRate { get; set; }
        public double? CloseProfit { get; set; }
        public double? CloseProfitPercentage { get; set; }

        public double CurrentRate { get; set; }
        public double StakeAmount { get; set; }
        public double Quantity { get; set; }

        public bool IsOpen { get; set; }

        public DateTime OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }

        public int Duration
        {
            get
            {
                if (CloseDate.HasValue)
                    return (CloseDate.Value - OpenDate).Minutes;
                else
                    return 0;
            }
        }

        public string Uuid { get; set; }

        public bool HasCurrentProfit { get { return CurrentProfit > 0; } }
        public bool HasProfit { get { return CloseProfit > 0; } }

        public double CurrentProfit
        {
            get
            {
                return CurrentRate - OpenRate;
            }
        }

        public double CurrentProfitPercentage
        {
            get
            {
                return ((CurrentRate - OpenRate) / CurrentRate) * 100;
            }
        }
    }
}
