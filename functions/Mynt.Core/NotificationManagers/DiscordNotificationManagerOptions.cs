namespace Mynt.Core.NotificationManagers
{
    public class DiscordNotificationManagerOptions : BaseSettings
    {
        public string DiscordWebhookId { get; set; }
        public string DiscordWebhookToken { get; set; }

        public DiscordNotificationManagerOptions()
        {
            TrySetFromConfig(() => DiscordWebhookId = AppSettings.Get<string>(nameof(DiscordWebhookId)));
            TrySetFromConfig(() => DiscordWebhookToken = AppSettings.Get<string>(nameof(DiscordWebhookToken)));
        }
    }
}