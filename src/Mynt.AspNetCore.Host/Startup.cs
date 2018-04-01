using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mynt.AspNetCore.Host.Hosting;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Interfaces;
using Mynt.Core.Notifications;
using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using Mynt.Data.AzureTableStorage;

namespace Mynt.AspNetCore.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // TODO do your own log4net provider that supports .net core 2.0 ILoggingBuilder
            // As for now, logging is configured in the method below
            /*services
                .AddLogging(b =>
                {
                    b
                        .AddDebug()
                        .AddConsole();
                });
                */
            services.AddMvc();

            // Set up exchange - TBD TODO more elegant solution
            var binance = Configuration.GetSection("Binance").Get<ExchangeOptions>();
            var bittrex = Configuration.GetSection("Bittrex").Get<ExchangeOptions>();
            if (!String.IsNullOrEmpty(bittrex?.ApiKey))
            {
                bittrex.Exchange = Exchange.Bittrex;
                services.AddSingleton<IExchangeApi>(i => new BaseExchange(bittrex));
            }
            else
            {
                binance.Exchange = Exchange.Binance;
                services.AddSingleton<IExchangeApi>(i => new BaseExchange(binance));
            }

            // Major TODO, coming soon
                services.AddSingleton<ITradingStrategy, TheScalper>()
                .AddSingleton<INotificationManager, TelegramNotificationManager>()
                    .AddSingleton(i => Configuration.GetSection("Telegram").Get<TelegramNotificationOptions>()) // TODO

                .AddSingleton<IDataStore, AzureTableStorageDataStore>()
                    .AddSingleton(i => Configuration.GetSection("AzureTableStorage").Get<AzureTableStorageOptions>()) // TODO
                .AddSingleton<ITradeManager, PaperTradeManager>()
                .AddSingleton(i => new TradeOptions())

                .AddSingleton<ILogger>(i => i.GetService<ILogger<ITradeManager>>())
                .AddSingleton<IHostedService>(i => new MyntHostedService(i.GetService<ILogger<MyntHostedService>>(), TimeSpan.FromHours(1), TimeSpan.Zero, TimeSpan.FromMinutes(1), i.GetService<ITradeManager>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // TODO see above method
            loggerFactory.AddConsole()
                .AddDebug()
                .AddLog4Net();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseMvc();
        }
    }
}