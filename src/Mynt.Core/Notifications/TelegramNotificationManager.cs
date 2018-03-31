using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mynt.Core.Notifications
{
    public class TelegramNotificationManager : INotificationManager
    {

        private readonly string _telegramWebhookUrl;
        private readonly string _chatId;

        public TelegramNotificationManager(TelegramNotificationOptions settings)
        {
            _chatId = settings.TelegramChatId;
            _telegramWebhookUrl = $"https://api.telegram.org/bot{settings.TelegramBotToken}/sendMessage";
        }

        public async Task<bool> SendNotification(string message)
        {
            try
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"chat_id", _chatId},
                    {"parse_mode", "Markdown"},
                    {"text", message}
                };

                var json = JsonConvert.SerializeObject(dictionary);
                json = json.Replace(@"\\n", @"\n");

                var requestData = new StringContent(json, Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(string.Format(_telegramWebhookUrl), requestData);

                    return true;
                }
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
    }
}