using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Mynt.Core.Configuration;
// using Microsoft.Extensions.Configuration;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Notifications;
using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using Mynt.Data.AzureTableStorage;
using Serilog;
using Serilog.Sinks.AzureWebJobsTraceWriter;

namespace Mynt.Functions
{
    public static class BuyTimer
    {
        static BuyTimer()
        {
            ApplicationHelper.Startup();
        }

        [FunctionName("BuyTimer")]
        public static async Task Run([TimerTrigger("10 1 * * * *")]TimerInfo buyTimer, TraceWriter log)
        {
            var serilogger = new LoggerConfiguration().WriteTo.TraceWriter(log).CreateLogger();
            var logger = new LoggerFactory().AddSerilog(serilogger).CreateLogger(nameof(BuyTimer));

            try
            {
                logger.LogInformation("Starting processing...");

                // Either use the default options as defined in TradeOptions or override them.
                // You can override them using the property setters here or by providing keys in your configuration mechanism
                // matching the property names in this class.

                var tradeOptions = AppSettings.Get<TradeOptions>();

                var exchangeOptions = AppSettings.Get<ExchangeOptions>();
                var azureTableStorageOptions = AppSettings.Get<AzureTableStorageOptions>();
                var telegramNotificationOptions = AppSettings.Get<TelegramNotificationOptions>();

                // logger.Information("Using trade options {@Options}", tradeOptions);

                // Initialize a Trade Manager instance that will run using the settings provided below.
                // Once again, you can use the default values for the settings defined in te Options classes below.
                // You can also override them here or using the configuration mechanism of your choosing.
                var tradeManager = new PaperTradeManager(
                    api: new BaseExchange(exchangeOptions),
                    dataStore: new AzureTableStorageDataStore(azureTableStorageOptions),
                    logger: logger,
                    notificationManager: new TelegramNotificationManager(telegramNotificationOptions),
                    settings: tradeOptions,
                    strategy: ApplicationHelper.TryCreateTradingStrategy(tradeOptions.DefaultStrategy) ?? new TheScalper());

                // Start running this thing!
                await tradeManager.LookForNewTrades();

                logger.LogInformation("Done...");
            }
            catch (Exception ex)
            {
                // If anything goes wrong log an error to Azure.
                logger.LogError(ex, "Error on BuyTimer");

                // TODO necessary?
//                if (ex.InnerException != null)
//                    logger.Error(ex.InnerException.Message + ex.InnerException.StackTrace);
            }
        }
    }
}
