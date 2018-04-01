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
    public class BuyTimer : IJob
    {
        public virtual Task Execute(IJobExecutionContext context)
        {
            var loggerFactory = new LoggerFactory().AddConsole();
            var logger = loggerFactory.CreateLogger<PaperTradeManager>();

            logger.LogInformation("Checking buy side...");

            var tradeManager = new PaperTradeManager(
                  api: new BaseExchange(new ExchangeOptions(Exchange.Binance)),
                  dataStore: new AzureTableStorageDataStore(new AzureTableStorageOptions()),
                  logger: logger,
                  notificationManager: new TelegramNotificationManager(new TelegramNotificationOptions()),
                  settings: new TradeOptions(),
                  strategy: new TheScalper());

            tradeManager.LookForNewTrades().GetAwaiter().GetResult();

            return Task.FromResult(true);
        }
    }
}
