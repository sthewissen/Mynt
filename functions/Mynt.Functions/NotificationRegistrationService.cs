using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Mynt.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Mynt.Core.Managers;
using Mynt.Core.NotificationManagers;
using Newtonsoft.Json;

namespace Mynt.Functions
{
    public static class NotificationRegistrationService
    {
        [FunctionName("NotificationRegistrationService")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications")]HttpRequestMessage req, TraceWriter log)
        {
            //read json object from request body
            var content = req.Content;
            var jsonContent = content.ReadAsStringAsync().Result;
            var installation = JsonConvert.DeserializeObject<Installation>(jsonContent);

            var manager = new PushNotificationManager();
            await manager.RegisterDevice(installation);

            return req.CreateResponse(HttpStatusCode.OK, installation.InstallationId);
        }
    }
}
