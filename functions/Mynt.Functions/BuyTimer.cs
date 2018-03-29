using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;

namespace Mynt.Functions
{
    public static class BuyTimer
    {
        [FunctionName("BuyTimer")]
        public static async Task Run([TimerTrigger("10 1 * * * *")]TimerInfo buyTimer, TraceWriter log)
        {
            try
            {
                log.Info("Starting processing...");

                var exchange = new BaseExchange(new ExchangeOptions(Exchange.Binance));
                var balance = await exchange.GetBalance("BTC");

                log.Info($"Current BTC balance: {balance.Balance.ToString()}");

                log.Info("Done...");
            }
            catch (Exception ex)
            {
                // If anything goes wrong log an error to Azure.
                log.Error(ex.Message + ex.StackTrace);
            }
        }
    }
}
