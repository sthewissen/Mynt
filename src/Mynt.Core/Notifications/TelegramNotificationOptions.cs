using Mynt.Core.Configuration;

namespace Mynt.Core.Notifications
{
    public class TelegramNotificationOptions : BaseSettings
    {
        public string TelegramChatId { get; set; }
        public string TelegramBotToken { get; set; }

        public TelegramNotificationOptions()
        {
            TrySetFromConfig(() => TelegramChatId = AppSettings.Get<string>(nameof(TelegramChatId)));
            TrySetFromConfig(() => TelegramBotToken = AppSettings.Get<string>(nameof(TelegramBotToken)));
        }
    }
}