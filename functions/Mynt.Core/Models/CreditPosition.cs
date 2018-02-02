using System;
using System.Reflection;
using log4net;

namespace Mynt.Core.Models
{
    public class CreditPosition
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            log.Info($"Registered a buy for {symbol}. Substracted {quantity * rate * (1 + fee)} from the credit. New BTC credit: {btcCredit}");
        }

        public void RegisterSell(double quantity, double rate)
        {
            // Increased the credit
            btcCredit += quantity * rate * (1 - fee);
            log.Info($"Registered a sell for {symbol}. Added {quantity * rate * (1 - fee)} to the credit. New BTC credit: {btcCredit}");

        }
    }
}
