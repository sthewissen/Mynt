using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Interfaces;
using Newtonsoft.Json;

namespace Mynt.Core.Notifications
{
    public class SlackNotificationManager : INotificationManager
    {
        private readonly string _slackWebhookUrl;

        public SlackNotificationManager(SlackNotificationOptions settings)
        {
            _slackWebhookUrl = settings.SlackWebhookUrl;
        }

        public async Task<bool> SendNotification(string message)
        {
            try
            {
                var payload = new Payload() { text = message };
                var payloadJson = JsonConvert.SerializeObject(payload);

                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(_slackWebhookUrl, new StringContent(payloadJson, Encoding.UTF8, "application/json"));

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
            public string text { get; set; }
        }
    }
}
