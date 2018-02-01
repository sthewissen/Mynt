using System;

namespace Mynt.Core.Models
{
    public class CreditPosition
    {
        private readonly string symbol;

        private readonly double fee;

        private double btcCredit;

        public CreditPosition(string symbol, double fee, double btcCredit)
        {
            this.symbol = symbol;
            this.fee =fee;
            this.btcCredit= btcCredit;
        }

        public string Symbol => symbol;

        public double BtcCredit => btcCredit;

        public void RegisterBuy(double quantity, double rate)
        {
            // Decreased the credit
            btcCredit -= quantity * rate * (1 + fee);
        }

        public void RegisterSell(double quantity, double rate)
        {
            // Increased the credit
            btcCredit += quantity * rate * (1 - fee);
            throw new NotImplementedException();
        }
    }
}
