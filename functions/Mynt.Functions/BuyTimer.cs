using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Mynt.Functions
{
    public static class BuyTimer
    {
        [FunctionName("BuyTimer")]
        public static async Task Run([TimerTrigger("10 1 * * * *")]TimerInfo buyTimer, TraceWriter log)
        {
            try
            {
                log.Info("Starting processing...");



                log.Info("Done...");
            }
            catch (Exception ex)
            {
                // If anything goes wrong log an error to Azure.
                log.Error(ex.Message + ex.StackTrace);
            }
        }
    }
}
}
