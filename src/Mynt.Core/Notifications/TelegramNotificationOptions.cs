using Mynt.Core.Configuration;

namespace Mynt.Core.Notifications
{
    public class TelegramNotificationOptions : BaseSettings
    {
        public string ChatId { get; set; }
        public string BotToken { get; set; }

        public TelegramNotificationOptions()
        {
            TrySetFromConfig(() => ChatId = AppSettings.Get<string>("TelegramChatId"));
            TrySetFromConfig(() => BotToken = AppSettings.Get<string>("TelegramBotToken"));
        }
    }
}