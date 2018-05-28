﻿using System;
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
using Mynt.Data.SqlServer;
using Mynt.Data.AzureTableStorage;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Mynt.AspNetCore.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure serilog from appsettings.json
            var serilogger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            services.AddLogging(b => { b.AddSerilog(serilogger); });

            services.AddMvc();

            // Set up exchange - TODO
            var exchangeOptions = Configuration.Get<ExchangeOptions>();
            exchangeOptions.Exchange = Exchange.Binance;
            services.AddSingleton<IExchangeApi>(i => new BaseExchange(exchangeOptions));

            var tradeOptions = Configuration.GetSection("TradeOptions").Get<TradeOptions>();

            var type = Type.GetType($"Mynt.Core.Strategies.{tradeOptions.DefaultStrategy}, Mynt.Core", true, true);

            // Major TODO, coming soon
            services.AddSingleton(s => Activator.CreateInstance(type) as ITradingStrategy ?? new TheScalper())
                .AddSingleton<INotificationManager, TelegramNotificationManager>()
                .AddSingleton(i => Configuration.GetSection("Telegram").Get<TelegramNotificationOptions>()) // TODO

                //.AddSingleton<IDataStore, SqlServerDataStore>()
                //.AddSingleton(i => Configuration.GetSection("SqlServerOptions").Get<SqlServerOptions>()) // TODO

                .AddSingleton<IDataStore, AzureTableStorageDataStore>()
                .AddSingleton(i => Configuration.GetSection("AzureTableStorageOptions").Get<AzureTableStorageOptions>()) // TODO
                .AddSingleton<ITradeManager, PaperTradeManager>()
                .AddSingleton(i => tradeOptions)

                .AddSingleton<ILogger>(i => i.GetRequiredService<ILoggerFactory>().CreateLogger<MyntHostedService>())

                .AddSingleton<IHostedService, MyntHostedService>()
                .Configure<MyntHostedServiceOptions>(Configuration.GetSection("Hosting"))

                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Mynt}/{action=Dashboard}/{id?}");
            });
        }
    }
}