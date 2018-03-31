using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Interfaces;
using Newtonsoft.Json;

namespace Mynt.Core.Notifications
{
    public class DiscordNotificationManager : INotificationManager
    {
        private readonly string _discordWebhookUrl;

        public DiscordNotificationManager(DiscordNotificationOptions settings)
        {
            _discordWebhookUrl = $"https://discordapp.com/api/webhooks/{settings.DiscordWebhookId}/{settings.DiscordWebhookToken}";
        }

        public async Task<bool> SendNotification(string message)
        {
            try
            {
                var payload = new Payload() { content = message };
                var payloadJson = JsonConvert.SerializeObject(payload);

                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(_discordWebhookUrl,
                    new StringContent(payloadJson, Encoding.UTF8, "application/json"));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendTemplatedNotification(string template, params object[] parameters)
        {
            var finalMessage = string.Format(template, parameters);
            return await SendNotification(finalMessage);
        }

        public class Payload
        {
            public string content { get; set; }
        }
    }
}
