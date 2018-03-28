using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Mynt.Functions
{
    public static class SellTimer
    {
        [FunctionName("SellTimer")]
        public static async Task Run([TimerTrigger("0 * * * * *")]TimerInfo sellTimer, TraceWriter log)
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
