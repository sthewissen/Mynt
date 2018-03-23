using System;
namespace Mynt.Core.NotificationManagers
{
    public class SlackNotificationManagerOptions : BaseSettings
    {
        public string SlackWebhookUrl { get; set; }

        public SlackNotificationManagerOptions()
        {
            TrySetFromConfig(() => SlackWebhookUrl = AppSettings.Get<string>(nameof(SlackWebhookUrl)));
        }
    }
}
