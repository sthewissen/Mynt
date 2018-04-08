using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using Quartz;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mynt.Core.Configuration;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Interfaces;
using Mynt.Core.Notifications;
using Mynt.Data.AzureTableStorage;

namespace Mynt.WindowsService
{
    [DisallowConcurrentExecution]
    public class TradeJob : IJob
    {
        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual Task Execute(IJobExecutionContext context)
        {
            var loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug()
                .AddLog4Net();
            var logger = loggerFactory.CreateLogger<TradeJob>();

            logger.LogInformation("Ready to shuffle your g0ldz!");

            var binanceSettings = new ExchangeOptions(Exchange.Binance);
            var bittrexSettings = new ExchangeOptions(Exchange.Bittrex);
            var bitfinexSettings = new ExchangeOptions(Exchange.Bitfinex);
            var poloniexSettings = new ExchangeOptions(Exchange.Poloniex);

            IExchangeApi api = null;

            // Use an API key if provided (one of these should be set)
            if (!String.IsNullOrEmpty(bittrexSettings.ApiKey))
                api = new BaseExchange(bittrexSettings);
            else if (!String.IsNullOrEmpty(binanceSettings.ApiKey))
                api = new BaseExchange(binanceSettings);
            else if (!String.IsNullOrEmpty(bitfinexSettings.ApiKey))
                api = new BaseExchange(bitfinexSettings);
            else if (!String.IsNullOrEmpty(poloniexSettings.ApiKey))
                api = new BaseExchange(poloniexSettings);

            
            // Default strategy
            var strategy = new TheScalper();

            // Use Telegram if settings set up in config
            INotificationManager notificationManager = null;
            var telegramOptions = new TelegramNotificationOptions();
            if (!String.IsNullOrWhiteSpace(telegramOptions.TelegramChatId))
            {
                notificationManager = new TelegramNotificationManager(telegramOptions);
            }

            var tradeSettings = new TradeOptions();

            if (!tradeSettings.PaperTrading)
            {
                // Call the Trade manager with the strategy of our choosing.
                var manager = new LiveTradeManager(api, strategy, notificationManager, logger, tradeSettings, new AzureTableStorageDataStore(new AzureTableStorageOptions()));

                // Call the process method to start processing the current situation.
                manager.LookForNewTrades().GetAwaiter().GetResult();
            }
            else
            {
                // Initialize a Trade Manager instance that will run using the settings provided below.
                // Once again, you can use the default values for the settings defined in te Options classes below.
                // You can also override them here or using the configuration mechanism of your choosing.
                var manager = new PaperTradeManager(api, strategy, notificationManager, logger, tradeSettings, new AzureTableStorageDataStore(new AzureTableStorageOptions()));

                // Start running this thing!
                manager.LookForNewTrades().GetAwaiter().GetResult();
            }
            return Task.FromResult(true);
        }
    }
}