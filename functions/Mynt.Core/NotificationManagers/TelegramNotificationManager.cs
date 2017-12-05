using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mynt.Core.NotificationManagers
{
    public class TelegramNotificationManager : INotificationManager
    {
        
        private readonly string TelegramWebhookUrl = "https://api.telegram.org/botTOKEN/sendMessage";
        
        public async Task<bool> SendNotification(string message)
        {
            try
            {
                
            var payload = new Payload() { text = message };
            var payloadJson = JsonConvert.SerializeObject(payload);
            var httpClient = new HttpClient();

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("chat_id", "CHATID");
            dictionary.Add("text", message);
            string json = JsonConvert.SerializeObject(dictionary);

            var requestData = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(string.Format(TelegramWebhookUrl), requestData);

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
