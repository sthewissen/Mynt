using System;
using System.Threading.Tasks;
using Microsoft.Azure.NotificationHubs;
using Mynt.Core.Interfaces;

namespace Mynt.Core.Managers
{
    public class NotificationManager : INotificationManager
    {
        public async Task<string> RegisterDevice(Installation installation)
        {
            var hub = NotificationHubClient.CreateClientFromConnectionString(Constants.NotificationAccessKey, Constants.NotificationHubName);
            await hub.CreateOrUpdateInstallationAsync(installation);
            return installation.InstallationId;
        }

        public async Task<bool> SendNotification(string message)
        {
            try
            {
                var hub = NotificationHubClient.CreateClientFromConnectionString(Constants.NotificationAccessKey, Constants.NotificationHubName);

                // Send the notification
                var result = await hub.SendAppleNativeNotificationAsync("{\"aps\":{\"alert\":\"" + message + "\", \"title\":\"" + message + "\", \"sound\":\"bingbong.aiff\"}}");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
