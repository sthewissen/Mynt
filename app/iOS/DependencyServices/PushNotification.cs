using System;
using System.Threading.Tasks;
using Foundation;
using Mynt.Helpers;
using Mynt.Interfaces;
using Mynt.iOS.DependencyServices;
using Mynt.Models;
using Mynt.Services;
using Plugin.Connectivity;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

[assembly: Dependency(typeof(PushNotification))]
namespace Mynt.iOS.DependencyServices
{
    public class PushNotification : IPushNotification
    {
        public bool IsDeviceRemoteRegistered => Settings.IsDeviceRemoteRegistered;

        public Task<bool> RegisterForNotifications()
        {
            var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                new NSSet());

            UIApplication.SharedApplication.UnregisterForRemoteNotifications();

            // Register again
            UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
            UIApplication.SharedApplication.RegisterForRemoteNotifications();

            return Task.FromResult(true);
        }

        public async Task<bool> RegisterDevice(string deviceToken)
        {
            try
            {
                // Update registration
                var api = new MyntApi();

                // Set to false before trying.
                Settings.IsDeviceRemoteRegistered = false;

                // Only available when connected.
                if (CrossConnectivity.Current.IsConnected)
                {
                    var installation = new Installation()
                    {
                        InstallationId = Settings.CurrentUniqueId,
                        Platform = "apns",
                        PushChannel = deviceToken
                    };

                    // Create our own push registration record.
                    var id = await api.Register(installation);

                    // If these match the registration was successful
                    if (id != null)
                    {
                        // Succesfully registered!
                        Settings.IsDeviceRemoteRegistered = true;

                        // Set the new device token.
                        Settings.DeviceToken = deviceToken;

                        return true;
                    }

                    return false;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
