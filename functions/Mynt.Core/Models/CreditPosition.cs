using log4net;
using System.Reflection;

namespace Mynt.Core.Models
{
    public class CreditPosition
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string symbol;

        private readonly double fee;

        private double balance;

        private double btcCredit;

        public CreditPosition(string symbol, double fee, double balance, double btcCredit)
        {
            this.symbol = symbol;
            this.fee = fee;
            this.balance = balance;
            this.btcCredit = btcCredit;
        }

        public string Symbol => symbol;

        public double BtcCredit => btcCredit;

        public void RegisterBuy(double quantity, double rate)
        {
            // Decreased the credit
            var buyValue = quantity * rate ;
            balance += buyValue;
            btcCredit -= buyValue * (1 + fee);
            log.Info($"Registered a buy for {symbol}. Subtracted {quantity * rate * (1 + fee):#0.##########} from the credit. New position = {balance:#0.##########}, BTC credit = {btcCredit:#0.##########}");
        }

        public void RegisterSell(double quantity, double rate)
        {
            // Increased the credit
            var sellValue = quantity * rate;
            balance -= sellValue;
            btcCredit += sellValue * (1 - fee);
            log.Info($"Registered a sell for {symbol}. Added {quantity * rate * (1 - fee):#0.##########} to the credit. New position = {balance:#0.##########}, BTC credit = {btcCredit:#0.##########}");

        }
    }
}
