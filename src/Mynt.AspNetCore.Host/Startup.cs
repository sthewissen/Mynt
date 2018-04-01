using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mynt.AspNetCore.WindowsService.Hosting;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Interfaces;
using Mynt.Core.Notifications;
using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using Mynt.Data.AzureTableStorage;

namespace Mynt.AspNetCore.WindowsService
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
            services
                .AddLogging(b =>
                {
                    b
                        .AddDebug()
                        .AddConsole();
                });

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

                services.AddSingleton<ITradingStrategy, TheScalper>()
                .AddSingleton<INotificationManager, TelegramNotificationManager>()
                    .AddSingleton<TelegramNotificationOptions>() // TODO

                .AddSingleton<IDataStore, AzureTableStorageDataStore>()
                    .AddSingleton<AzureTableStorageOptions>() // TODO
                .AddSingleton<LiveTradeManager>()
                .AddSingleton(i => new TradeOptions())

                .AddSingleton<ILogger>(i => i.GetService<ILogger<LiveTradeManager>>())
                .AddSingleton<IHostedService>(i => new TimedHostedService(i.GetService<ILogger<TimedHostedService>>(), TimeSpan.FromHours(1), TimeSpan.Zero, TimeSpan.FromMinutes(1), i.GetService<LiveTradeManager>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}