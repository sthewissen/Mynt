using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using Quartz;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

            IExchangeApi api;
            // Use Bittrex if API Key provided
            // or Binance if it's not (one of both should be set)
            if (!String.IsNullOrEmpty(bittrexSettings.ApiKey))
            {
                api = new BaseExchange(bittrexSettings);
            }
            else
            {
                api = new BaseExchange(binanceSettings);
            }

            
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
            // Call the Trade manager with the strategy of our choosing.
            var manager = new GenericTradeManager(api, strategy, notificationManager, logger, tradeSettings, new AzureTableStorageDataStore(new AzureTableStorageOptions()));

            // Call the process method to start processing the current situation.
            manager.LookForNewTrades().GetAwaiter().GetResult();

            return Task.FromResult(true);
        }
    }
}