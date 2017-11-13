using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Mynt.Core;
using Mynt.Core.Managers;
using Mynt.Core.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Mynt.Functions.Dto;

namespace Mynt.Functions
{
    public static class SettingsService
    {
        [FunctionName("SettingsService")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "settings")]HttpRequestMessage req, string key, TraceWriter log)
        {
            // Fetching the name from the path parameter in the request URL
            return req.CreateResponse(HttpStatusCode.OK, new SettingsDto()
            {
                AmountOfWorkers = Constants.MaxNumberOfConcurrentTrades,
                StakePerWorker = Constants.AmountOfBtcToInvestPerTrader,
                StopLossPercentage = Constants.StopLossPercentage,
                MinimumAmountOfVolume = Constants.MinimumAmountOfVolume
            });
        }
    }
}
