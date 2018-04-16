using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Mynt.Core.Configuration;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Data.AzureTableStorage;

namespace Mynt.TelegramBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;

        public Task StartAsync(IDialogContext context)
        {
            try
            {
                context.Wait(MessageReceivedAsync);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }

            return Task.CompletedTask;
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            switch (message.Text.Split(' ').FirstOrDefault())
            {
                case "/trades":
                    var returnMessage = await CreateTradeString();
                    await context.PostAsync($"{returnMessage}");
                    context.Wait(MessageReceivedAsync);
                    break;
                case "/profit":
                    var profitMessage = await CreateProfitString();
                    await context.PostAsync($"{profitMessage}");
                    context.Wait(MessageReceivedAsync);
                    break;
            }
        }

        private async Task<string> CreateProfitString()
        {
            var azureTableStorageOptions = AppSettings.Get<AzureTableStorageOptions>();
            var dataStore = new AzureTableStorageDataStore(azureTableStorageOptions);
            await dataStore.InitializeAsync();

            var traders = await dataStore.GetTradersAsync();

            if (traders.Count == 0) 
                return "No profits yet, patience...";

            var balance = 0.0m;
            var stake = 0.0m;

            foreach (var item in traders)
            {
                balance += item.CurrentBalance;
                stake += item.StakeAmount;
            }

            return $"Current profit is {(balance-stake):0.00000000} BTC ({(((balance-stake)/stake) * 100):0.00)}%)";
        }

        private async Task<string> CreateTradeString()
        {
            var exchangeOptions = AppSettings.Get<ExchangeOptions>();
            var azureTableStorageOptions = AppSettings.Get<AzureTableStorageOptions>();

            var dataStore = new AzureTableStorageDataStore(azureTableStorageOptions);
            await dataStore.InitializeAsync();

            var trades = await dataStore.GetActiveTradesAsync();
            var exchange = new BaseExchange(exchangeOptions);
            var stringResult = new StringBuilder();

            foreach (var item in trades)
            {
                var ticker = await exchange.GetTicker(item.Market);
                var currentProfit = ((ticker.Bid - item.OpenRate) / item.OpenRate) * 100;
                stringResult.AppendLine($"**{item.Market}:** {currentProfit:0.00}% opened {item.OpenDate.Humanize()}\n");
            }

            if (trades.Count == 0)
                stringResult.Append("No current active trades...");

            return stringResult.ToString();
        }
    }
}
