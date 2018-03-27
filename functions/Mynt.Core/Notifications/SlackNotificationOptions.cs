using System;
using Mynt.Core.Configuration;

namespace Mynt.Core.Notifications
{
    public class SlackNotificationOptions : BaseSettings
    {
        public string SlackWebhookUrl { get; set; }

        public SlackNotificationOptions()
        {
            TrySetFromConfig(() => SlackWebhookUrl = AppSettings.Get<string>(nameof(SlackWebhookUrl)));
        }
    }
}
