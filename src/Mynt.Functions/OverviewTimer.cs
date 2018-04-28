using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Mynt.Core.Configuration;
using Mynt.Core.Exchanges;
using Mynt.Core.Interfaces;
using Mynt.Core.Notifications;
using Mynt.Data.AzureTableStorage;
using Serilog;
using Serilog.Sinks.AzureWebJobsTraceWriter;

namespace Mynt.Functions
{
    public static class OverviewTimer
    {
        static OverviewTimer()
        {
            ApplicationHelper.Startup();
        }

        [FunctionName("OverviewTimer")]
        public static async Task Run([TimerTrigger("0 0 * * * *")]TimerInfo overviewTimer, TraceWriter log)
        {
            var logger = new LoggerConfiguration().WriteTo.TraceWriter(log).CreateLogger();

            try
            {
                logger.Information("Starting processing...");

                // Either use the default options as defined in TradeOptions or override them.
                // You can override them using the property setters here or by providing keys in your configuration mechanism
                // matching the property names in this class.
                var azureTableStorageOptions = AppSettings.Get<AzureTableStorageOptions>();
                var telegramNotificationOptions = AppSettings.Get<TelegramNotificationOptions>();

                var dataStore = new AzureTableStorageDataStore(azureTableStorageOptions);
                await dataStore.InitializeAsync();

                var telegram = new TelegramNotificationManager(telegramNotificationOptions);

                await SendTradeOverviewMessage(telegram, dataStore);
                await SendProfitText(telegram, dataStore);

                logger.Information("Done...");
            }
            catch (Exception ex)
            {
                // If anything goes wrong log an error to Azure.
                logger.Error(ex.Message + ex.StackTrace);

                if (ex.InnerException != null)
                    logger.Error(ex.InnerException.Message + ex.InnerException.StackTrace);
            }
        }

        private static async Task SendProfitText(INotificationManager notificationManager, IDataStore dataStore)
        {
            var traders = await dataStore.GetTradersAsync();

            if (traders.Count > 0)
            {
                var balance = traders.Sum(x => x.CurrentBalance);
                var stake = traders.Sum(x => x.StakeAmount);

                if (balance - stake == 0)
                    return;

                await notificationManager.SendNotification($"Current profit is {(balance - stake):0.00000000} BTC ({(((balance - stake) / stake) * 100):0.00}%)");
            }
        }

        private static async Task SendTradeOverviewMessage(INotificationManager notificationManager, IDataStore dataStore)
        {
            var trades = await dataStore.GetActiveTradesAsync();

            if (trades.Count > 0)
            {
                var exchangeOptions = AppSettings.Get<ExchangeOptions>();
                var exchange = new BaseExchange(exchangeOptions);
                var stringResult = new StringBuilder();

                foreach (var item in trades)
                {
                    var ticker = await exchange.GetTicker(item.Market);
                    var currentProfit = ((ticker.Bid - item.OpenRate) / item.OpenRate) * 100;
                    stringResult.AppendLine($"#{item.Market}: *{currentProfit:0.00}%* opened {item.OpenDate.Humanize()} at {item.OpenRate:0.00000000} BTC");
                }

                await notificationManager.SendNotification(stringResult.ToString());
            }
        }
    }
}