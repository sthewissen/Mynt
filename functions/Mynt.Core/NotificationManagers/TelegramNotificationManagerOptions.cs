namespace Mynt.Core.NotificationManagers
{
    public class TelegramNotificationManagerOptions : BaseSettings
    {
        public string TelegramChatId { get; set; }
        public string TelegramBotToken { get; set; }

        public TelegramNotificationManagerOptions()
        {
            TrySetFromConfig(() => TelegramChatId = AppSettings.Get<string>(nameof(TelegramChatId)));
            TrySetFromConfig(() => TelegramBotToken = AppSettings.Get<string>(nameof(TelegramBotToken)));
        }
    }
}