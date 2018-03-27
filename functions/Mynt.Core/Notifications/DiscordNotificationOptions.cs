using Mynt.Core.Configuration;

namespace Mynt.Core.Notifications
{
    public class DiscordNotificationOptions : BaseSettings
    {
        public string DiscordWebhookId { get; set; }
        public string DiscordWebhookToken { get; set; }

        public DiscordNotificationOptions()
        {
            TrySetFromConfig(() => DiscordWebhookId = AppSettings.Get<string>(nameof(DiscordWebhookId)));
            TrySetFromConfig(() => DiscordWebhookToken = AppSettings.Get<string>(nameof(DiscordWebhookToken)));
        }
    }
}