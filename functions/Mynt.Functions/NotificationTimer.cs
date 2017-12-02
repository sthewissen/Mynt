using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Mynt.Core.NotificationManagers;
using Mynt.Core.Strategies;

namespace Mynt.Functions
{
    public static class NotificationTimer
    {
        [FunctionName("NotificationTimer")]
        public static async Task Run([TimerTrigger("10 0 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            try
            {
                var trendManager = new NotificationManager(new AwesomeSma(), (a) => log.Info(a));
                await trendManager.Process();
            }
            catch (Exception ex)
            {
                // If anything goes wrong log an error to Azure.
                log.Error(ex.Message + ex.StackTrace);
            }
}
    }
}
