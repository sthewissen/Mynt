using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Notifications;
using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using Mynt.Data.AzureTableStorage;
using Quartz;
using Quartz.Impl;

namespace Mynt.Console
{
    class Program
    {
        private static IScheduler scheduler;

        static void Main(string[] args)
        {
            //var builder = new ConfigurationBuilder()
            //               .SetBasePath(Directory.GetCurrentDirectory())
            //               .AddJsonFile("appsettings.json");

            //var config = builder.Build();
            
            Start(args);

            System.Console.WriteLine("Press any key to stop...");
            System.Console.ReadKey(true);

            Stop();
        }

        private static void Start(string[] args)
        {
            InitAndStartScheduler().GetAwaiter().GetResult();
        }

        private static void Stop()
        {
            StopScheduler().GetAwaiter().GetResult();
        }

        private static async Task InitAndStartScheduler()
        {
            // First we must get a reference to a scheduler
            var schedulerFactory = new StdSchedulerFactory();
            scheduler = await schedulerFactory.GetScheduler();

            IJobDetail buyJob = JobBuilder.Create(typeof(BuyTimer)).WithIdentity("BuyTimer", "TradingGroup").Build();
            IJobDetail sellJob = JobBuilder.Create(typeof(SellTimer)).WithIdentity("SellTimer", "TradingGroup").Build();

            // Every 5 minutes, 10 seconds after the minute == "10 0/5 * * * ?".
            // Every 10 seconds (for debug purposes) == "0/10 * * * * ?"
            string buyCronTrigger = "10 1 * * * ?";
            string sellCronTrigger = "0 * * * * ?";

            ITrigger buyTrigger = TriggerBuilder.Create()
                .WithIdentity("BuyCronTrigger", "TradingGroup")
                .WithCronSchedule(buyCronTrigger)
                .Build();
            
            ITrigger sellTrigger = TriggerBuilder.Create()
                .WithIdentity("SellCronTrigger", "TradingGroup")
                .WithCronSchedule(sellCronTrigger)
                .Build();

            var dictionary = new Dictionary<IJobDetail, IReadOnlyCollection<ITrigger>>();
            dictionary.Add(buyJob, new HashSet<ITrigger>() { buyTrigger });
            dictionary.Add(sellJob, new HashSet<ITrigger>() { sellTrigger });

            // Tell quartz to schedule the job using our trigger
            await scheduler.ScheduleJobs(dictionary, true);

            // Start up the scheduler (nothing can actually run until the
            // scheduler has been started)
            await scheduler.Start();
        }

        private static async Task StopScheduler()
        {
            await scheduler.Shutdown(true);
        }
    }
}
