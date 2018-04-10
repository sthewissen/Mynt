using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Notifications;
using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using Mynt.Data.AzureTableStorage;
using Quartz;

namespace Mynt.Console
{
    [DisallowConcurrentExecution]
    public class SellTimer : IJob
    {
        public virtual Task Execute(IJobExecutionContext context)
        {
            var loggerFactory = new LoggerFactory().AddConsole();
            var logger = loggerFactory.CreateLogger<PaperTradeManager>();

            logger.LogInformation("Checking sell side...");

            var tradeManager = new PaperTradeManager(
                  api: new BaseExchange(new ExchangeOptions()),
                  dataStore: new AzureTableStorageDataStore(new AzureTableStorageOptions()),
                  logger: logger,
                  notificationManager: new TelegramNotificationManager(new TelegramNotificationOptions()),
                  settings: new TradeOptions(),
                  strategy: new TheScalper());

            tradeManager.UpdateExistingTrades().GetAwaiter().GetResult();

            return Task.FromResult(true);
        }
    }
}
