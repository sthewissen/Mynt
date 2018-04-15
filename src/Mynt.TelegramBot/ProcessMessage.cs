using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Mynt.TelegramBot
{
    public static class ProcessMessage
    {
        static ProcessMessage()
        {
            ApplicationHelper.Startup();
        }

        [FunctionName("message")]
        public static async Task<object> Run([HttpTrigger] HttpRequestMessage req, TraceWriter log)
        {
            // Initialize the azure bot
            using (BotService.Initialize())
            {
                // Deserialize the incoming activity
                string jsonContent = await req.Content.ReadAsStringAsync();
                var activity = JsonConvert.DeserializeObject<Activity>(jsonContent);

                // authenticate incoming request and add activity.ServiceUrl to MicrosoftAppCredentials.TrustedHostNames
                // if request is authenticated
                if (!await BotService.Authenticator.TryAuthenticateAsync(req, new[] { activity }, CancellationToken.None))
                {
                    return BotAuthenticator.GenerateUnauthorizedResponse(req);
                }

                if (activity != null)
                {
                    // one of these will have an interface and process it
                    switch (activity.GetActivityType())
                    {
                        case ActivityTypes.Message:
                            await Conversation.SendAsync(activity, () => new EchoDialog());
                            break;
                        case ActivityTypes.ConversationUpdate:
                        case ActivityTypes.ContactRelationUpdate:
                        case ActivityTypes.Typing:
                        case ActivityTypes.DeleteUserData:
                        case ActivityTypes.Ping:
                        default:
                            log.Error($"Unknown activity type ignored: {activity.GetActivityType()}");
                            break;
                    }
                }
                return req.CreateResponse(HttpStatusCode.Accepted);
            }
        }
    }
}
