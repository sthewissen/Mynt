using Mynt.Core;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.WindowsService
{
    // Inspired by https://stackoverflow.com/questions/7764088/net-console-application-as-windows-service
    class Program
    {       
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
#if DEBUG
            // Easy verification of which App.settings is used.
            log.Info("AppSettings.OverwritingExample: " + System.Configuration.ConfigurationManager.AppSettings["OverwritingExample"]);
#endif

            if (!Environment.UserInteractive)
            {
                // Running as service
                using (var service = new Service())
                    ServiceBase.Run(service);
            }
            else
            { 
                // Running as console app
                Start(args);

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);

                Stop();
            }
        }  

        private static void Start(string[] args)
        {
            // Useful when trying to debug this proces when it's being ran as a Windows Service
            if (!System.Diagnostics.Debugger.IsAttached && AppSettings.Get<bool>("RequestDebuggerOnStartup"))
            {
                System.Diagnostics.Debugger.Launch();
            }
            
            InitAndStartScheduler().GetAwaiter().GetResult();
        }

        private static void Stop()
        {
            StopScheduler().GetAwaiter().GetResult();
        }

        private static IScheduler scheduler;

        private static async Task InitAndStartScheduler()
        {
            // First we must get a reference to a scheduler
            var schedulerFactory = new StdSchedulerFactory();
            scheduler = await schedulerFactory.GetScheduler();

            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<TradeJob>()
                .WithIdentity("TradeJob", "TradingGroup")                  
                .Build();

            // Every 5 minutes, 10 seconds after the minute == "10 0/5 * * * ?".
            // Every 10 seconds (for debug purposes) == "0/10 * * * * ?"
            string cronTrigger = System.Configuration.ConfigurationManager.AppSettings["TradeJobCronTrigger"];
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("TradeJobTrigger", "TradingGroup")
                .WithCronSchedule(cronTrigger)                 
                .Build();

            // Tell quartz to schedule the job using our trigger
            await scheduler.ScheduleJob(job, trigger);
            log.Debug($"{job.Key} will run at: {trigger.GetNextFireTimeUtc() ?? DateTime.MinValue:r}");

            // Start up the scheduler (nothing can actually run until the
            // scheduler has been started)
            await scheduler.Start();
            log.Info("Started Scheduler");

            //// wait long enough so that the scheduler as an opportunity to
            //// run the job!
            //log.Info("Waiting 65 seconds...");

            //// wait 65 seconds to show jobs
            //await Task.Delay(TimeSpan.FromSeconds(65));
        }

        private static async Task StopScheduler()
        {
            // shut down the scheduler
            log.Info("Shutting Down");
            await scheduler.Shutdown(true);
            log.Info("Shutdown Complete");
        }

        #region Nested classes to support running as service
        public const string ServiceName = "Mynt Service";

        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = Program.ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                Program.Start(args);
            }

            protected override void OnStop()
            {
                Program.Stop();
            }
        }
        #endregion
    }
}
