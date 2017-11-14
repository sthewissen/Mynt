using System;
using Foundation;
using Mynt.Helpers;
using Mynt.Interfaces;
using UIKit; using UserNotifications;
using Xamarin.Forms;

namespace Mynt.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            LoadApplication(new App());

            OxyPlot.Xamarin.Forms.Platform.iOS.PlotViewRenderer.Init();

            // Set a light status bar.
            UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);

            if (Settings.CurrentUniqueId == string.Empty)
                Settings.CurrentUniqueId = Guid.NewGuid().ToString().Split('-')[0];

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var authOptions = UserNotifications.UNAuthorizationOptions.Alert | UserNotifications.UNAuthorizationOptions.Badge | UserNotifications.UNAuthorizationOptions.Sound;

                // Request notification permissions from the user
                UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (approved, err) =>
                {
                    // Handle approval
                    if (err != null)
                    {
                        Console.WriteLine(err.LocalizedFailureReason);
                        Console.WriteLine(err.LocalizedDescription);
                    }
                    else if (approved)
                    {
                        GetNotificationSettings();
                    }
                });
            }
            else
            {
                var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                        UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                        new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }

            return base.FinishedLaunching(app, options);
        }

        private void GetNotificationSettings()
        {
            UNUserNotificationCenter.Current.GetNotificationSettings((settings) =>
            {
                InvokeOnMainThread(() =>
               {
                   if (settings.AuthorizationStatus == UNAuthorizationStatus.Authorized)
                       UIApplication.SharedApplication.RegisterForRemoteNotifications();
               });
            });
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            // Register the device
            var pnsId = deviceToken.ToString().Replace("<", "").Replace(">", "").Replace(" ", "");

            // Re-register the device when the token we get is different from what we had.
            if (pnsId != Settings.DeviceToken)
            {
                Settings.DeviceToken = pnsId;
            }
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            Console.WriteLine($"Failed to register for push, {error.LocalizedFailureReason} {error.LocalizedDescription}");
        }

        // To receive notifications in foreground on iOS 9 and below.
        // To receive notifications in background in any iOS version
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                Console.WriteLine(@"iOS version >= 10. Let NotificationCenter handle this one.");
                // set a member variable to tell the new delegate that this is background  
                return;
            }

            Console.WriteLine($"HANDLE PUSH, didReceiveRemoteNotification: {userInfo}");

            // custom code to handle notification content  

            //if ( [UIApplication sharedApplication].applicationState == UIApplicationStateInactive)
            //{
            //    NSLog(@"INACTIVE");
            //    completionHandler(UIBackgroundFetchResultNewData);
            //}
            //else if ( [UIApplication sharedApplication].applicationState == UIApplicationStateBackground)
            //{
            //    NSLog(@"BACKGROUND");
            //    completionHandler(UIBackgroundFetchResultNewData);
            //}
            //else
            //{
            //    NSLog(@"FOREGROUND");
            //    completionHandler(UIBackgroundFetchResultNewData);
            //}
        }

        //void ProcessNotification(NSDictionary userInfo, bool fromLaunch)
        //{
        //    // If the notification has nothing, don't process.
        //    if (userInfo == null)
        //        return;

        //    var apsKey = new NSString("aps");
        //    var alertKey = new NSString("alert");

        //    // It has to have an aps key
        //    if (userInfo.ContainsKey(apsKey))
        //    {
        //        // Get the alert text
        //        var aps = (NSDictionary)userInfo.ObjectForKey(apsKey);

        //        if (aps.ContainsKey(alertKey))
        //        {
        //            // Show the alert
        //            var alert = (NSString)aps.ObjectForKey(alertKey);
        //        }
        //    }
        //}
    }

    public class UserNotificationCenterDelegate : UNUserNotificationCenterDelegate
    {
        public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            // Do something with the notification
            Console.WriteLine("Active Notification: {0}", notification);

            // Tell system to display the notification anyway or use
            // `None` to say we have handled the display locally.
            completionHandler(UNNotificationPresentationOptions.Alert);
        }

        public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            // Take action based on Action ID
            switch (response.ActionIdentifier)
            {
                case "reply":
                    // Do something
                    Console.WriteLine("Received the REPLY custom action.");
                    break;
                default:
                    // Take action based on identifier
                    if (response.IsDefaultAction)
                    {
                        // Handle default action...
                        Console.WriteLine("Handling the default action.");
                    }
                    else if (response.IsDismissAction)
                    {
                        // Handle dismiss action
                        Console.WriteLine("Handling a custom dismiss action.");
                    }
                    break;
            }

            // Inform caller it has been handled
            completionHandler();
        }
    }
}
