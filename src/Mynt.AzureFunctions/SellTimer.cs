using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Notifications;
using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using Mynt.Data.AzureTableStorage;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Sinks.AzureWebJobsTraceWriter;

namespace Mynt.AzureFunctions
{
    public static class SellTimer
    {
        static SellTimer()
        {
            ApplicationHelper.Startup();
        }

        [FunctionName("SellTimer")]
        public static async Task Run([TimerTrigger("0 * * * * *")]TimerInfo sellTimer, TraceWriter log, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var logger = new LoggerConfiguration().WriteTo.TraceWriter(log).CreateLogger();

            try
            {
                logger.Information("Starting processing...");

                // Either use the default options as defined in TradeOptions or override them.
                // You can override them using the properties in the TradeOptions class below.
                 var options = new TradeOptions()
                 {
                    MarketBlackList = new List<string> { "TRX", "XVG" }
                 };

                // Spin up something from Newtonsoft.Json...
                var obj = new Newtonsoft.Json.Linq.JObject();

                var exchangeOptions = config.Get<ExchangeOptions>();
                var azureTableStorageOptions = config.Get<AzureTableStorageOptions>();
                var telegramNotificationOptions = config.Get<TelegramNotificationOptions>();

                // Initialize a Trade Manager instance that will run using the settings provided below.
                // Once again, you can use the default values for the settings defined in te Options classes below.
                // You can also override them here or using the configuration mechanism of your choosing.
                var tradeManager = new PaperTradeManager(
                    api: new BaseExchange(exchangeOptions),
                    dataStore: new AzureTableStorageDataStore(azureTableStorageOptions),
                    logger: null,
                    notificationManager: new TelegramNotificationManager(telegramNotificationOptions),
                    settings: options,
                    strategy: new TheScalper());

                // Start running this thing!
                await tradeManager.UpdateExistingTrades();

                logger.Information("Done...");
            }
            catch (Exception ex)
            {
                // If anything goes wrong log an error to Azure.
                logger.Error(ex.Message + ex.StackTrace);

                if(ex.InnerException!=null)
                    logger.Error(ex.InnerException.Message + ex.InnerException.StackTrace);
            }
        }
    }
}
