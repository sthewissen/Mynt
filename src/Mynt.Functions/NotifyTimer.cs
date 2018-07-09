using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Mynt.Core.Configuration;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Notifications;
using Mynt.Core.Strategies;
using Mynt.Core.Strategies.Simple;
using Mynt.Core.TradeManagers;
using Mynt.Data.AzureTableStorage;
using Serilog;
using Serilog.Sinks.AzureWebJobsTraceWriter;

namespace Mynt.Functions
{
    public static class NotifyTimer
    {
        static NotifyTimer()
        {
            ApplicationHelper.Startup();
        }

        [Disable, FunctionName("NotifyTimer")]
        public static async Task Run([TimerTrigger("10 1 0,4,8,12,16,20 * * *")]TimerInfo notifyTimer, TraceWriter log)
        {
            var serilogger = new LoggerConfiguration().WriteTo.TraceWriter(log).CreateLogger();
            var logger = new LoggerFactory().AddSerilog(serilogger).CreateLogger(nameof(NotifyTimer));

            try
            {
                logger.LogInformation("Starting processing...");

                var tradeOptions = AppSettings.Get<TradeOptions>();
                var exchangeOptions = AppSettings.Get<ExchangeOptions>();
                var telegramNotificationOptions = AppSettings.Get<TelegramNotificationOptions>();

                // Initialize a Trade Manager instance that will run using the settings provided below.
                // Once again, you can use the default values for the settings defined in te Options classes below.
                // You can also override them here or using the configuration mechanism of your choosing.
                var tradeManager = new NotifyOnlyTradeManager(
                    new BaseExchange(exchangeOptions),
                    new TelegramNotificationManager(telegramNotificationOptions),
                    logger,
                    tradeOptions,
                    new CloudParty());

                // Start running this thing!
                await tradeManager.LookForNewTrades();

                logger.LogInformation("Done...");
            }
            catch (Exception ex)
            {
                // If anything goes wrong log an error to Azure.
                logger.LogError(ex, "Error on NotifyTimer");
            }
        }
    }
}
