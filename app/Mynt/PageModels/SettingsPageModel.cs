using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using FreshMvvm;
using Mynt.Helpers;
using Mynt.Interfaces;
using Mynt.Services;
using PropertyChanged;
using Xamarin.Forms;

namespace Mynt.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsPageModel : FreshBasePageModel
    {
        private MyntApi _api;

        public bool IsLoading { get; set; }

        public string FunctionRoot { get; set; }
        public int AmountOfWorkers { get; set; }
        public double StakePerWorker { get; set; }
        public double StopLossPercentage { get; set; }
        public int MinimumAmountOfVolume { get; set; }

        ICommand registerCommand;
        public ICommand RegisterCommand => registerCommand ?? (registerCommand = new Command(async () => await RegisterNotifications()));

        ICommand refreshCommand;
        public ICommand RefreshCommand => refreshCommand ?? (refreshCommand = new Command(async () => await RefreshData()));

        private async Task RegisterNotifications()
        {
            if (!string.IsNullOrEmpty(Settings.DeviceToken))
            {
                var push = DependencyService.Get<IPushNotification>();
                var result = await push.RegisterDevice(Settings.DeviceToken);

                Settings.FunctionRoot = FunctionRoot;

                if (result)
                {
                    Device.BeginInvokeOnMainThread(() =>
                   {
                       UserDialogs.Instance.Alert("Succesfully registered with your backend!", "Time to party", "OK");
                   });

                    await RefreshData();
                }
            }
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            Settings.FunctionRoot = FunctionRoot;
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            FunctionRoot = Settings.FunctionRoot;

            _api = new MyntApi();

            Task.Factory.StartNew(x => RefreshData(), null);
        }

        private async Task RefreshData()
        {
            IsLoading = true;

            if (Settings.FunctionRoot != string.Empty)
            {
                var settings = await _api.GetSettings();

                AmountOfWorkers = settings.AmountOfWorkers;
                StakePerWorker = settings.StakePerWorker;
                StopLossPercentage = settings.StopLossPercentage * 100;
                MinimumAmountOfVolume = settings.MinimumAmountOfVolume;
            }

            IsLoading = false;
        }
    }
}
