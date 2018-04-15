﻿using System;
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

            switch(message.Text.Split(' ').FirstOrDefault())
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
            // throw new NotImplementedException();
            return "";
        }

        private async Task<string> CreateTradeString()
        {
            var exchangeOptions = AppSettings.Get<ExchangeOptions>();
            var azureTableStorageOptions = AppSettings.Get<AzureTableStorageOptions>();

            var dataStore = new AzureTableStorageDataStore(azureTableStorageOptions);
            var trades = await dataStore.GetActiveTradesAsync();
            var exchange = new BaseExchange(exchangeOptions);
            var stringResult = new StringBuilder();

            foreach (var item in trades)
            {
                var ticker = await exchange.GetTicker(item.Market);
                var currentProfit = (ticker.Bid - item.OpenRate) / item.OpenRate;
                stringResult.AppendLine($"{item.Market} - {currentProfit:0.00}% - {item.OpenDate.Humanize()}");
            }

            return stringResult.ToString();
        }
    }
}