using System;
namespace Mynt.Models
{
    public class TradeSettings
    {
        public int AmountOfWorkers { get; set; }
        public double StakePerWorker { get; set; }
        public double StopLossPercentage { get; set; }
        public int MinimumAmountOfVolume { get; set; }
    }
}
