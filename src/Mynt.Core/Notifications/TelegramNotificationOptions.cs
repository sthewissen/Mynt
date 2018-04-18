using Mynt.Core.Configuration;

namespace Mynt.Core.Notifications
{
    public class TelegramNotificationOptions
    {
        public string TelegramChatId { get; set; }
        public string TelegramBotToken { get; set; }
    }
}