using FreshMvvm;
using Mynt.PageModels;
using Xamarin.Forms;
using System.Threading.Tasks;
using Mynt.Helpers;
using Mynt.Services;
using Plugin.Connectivity;
using Mynt.Models;

namespace Mynt
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var container = new FreshTabbedNavigationContainer();

            container.AddTab<CurrentTradePageModel>("Trades", "trades.png");
            container.AddTab<HistoryPageModel>("History", "history.png");
            container.AddTab<SettingsPageModel>("Settings", "settings.png");

            MainPage = container;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            //CrossPushNotification.Current.OnTokenRefresh += (s, p) =>
            //{
            //    Task.Factory.StartNew(() => RegisterForPush(p.Token));
            //};
        }

        private async Task RegisterForPush(string deviceToken)
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
                }
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
