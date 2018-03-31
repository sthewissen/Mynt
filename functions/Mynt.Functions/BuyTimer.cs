using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Notifications;
using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using Mynt.Data.AzureTableStorage;

namespace Mynt.Functions
{
    public static class BuyTimer
    {
        [FunctionName("BuyTimer")]
        public static async Task Run([TimerTrigger("10 1 * * * *")]TimerInfo buyTimer, ILogger log)
        {
            try
            {
                log.LogInformation("Starting processing...");

                // Either use the default options as defined in TradeOptions or override them.
                // You can override them using the property setters here or by providing keys in your configuration mechanism
                // matching the property names in this class.

                 var options = new TradeOptions()
                 {
                    MarketBlackList = new List<string> { "TRX", "XVG" }
                 };

                // Initialize a Trade Manager instance that will run using the settings provided below.
                // Once again, you can use the default values for the settings defined in te Options classes below.
                // You can also override them here or using the configuration mechanism of your choosing.
                var tradeManager = new PaperTradeManager(
                    api: new BaseExchange(new ExchangeOptions(Exchange.Binance)),
                    dataStore: new AzureTableStorageDataStore(new AzureTableStorageOptions()),
                    logger: log,
                    notificationManager: new TelegramNotificationManager(new TelegramNotificationOptions()),
                    settings: options,
                    strategy: new TheScalper());

                // Start running this thing!
                await tradeManager.LookForNewTrades();

                log.LogInformation("Done...");
            }
            catch (Exception ex)
            {
                // If anything goes wrong log an error to Azure.
                log.LogError(ex.Message + ex.StackTrace);
            }
        }
    }
}
