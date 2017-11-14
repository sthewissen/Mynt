using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using FreshMvvm;
using Mynt.Models;
using Mynt.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Xamarin.Forms;

namespace Mynt.PageModels
{
    public class TradePageModel : FreshBasePageModel
    {
        private MyntApi _api;
        private EuroApi _euroApi;
        private double _euroValue;
        private bool _isSelling;

        public bool ShowEuro { get; set; }
        public string Currency { get; private set; }
        public double Quantity { get; private set; }
        public double CurrentProfitPercentage { get; private set; }
        public double CurrentRate { get; private set; }
        public double CurrentProfit { get; private set; }
        public bool HasCurrentProfit { get; private set; }

        ICommand showEuroCommand;
        public ICommand ShowEuroCommand => showEuroCommand ?? (showEuroCommand = new Command(() => ShowEuros()));

        ICommand sellCommand;
        public ICommand SellCommand => sellCommand ?? (sellCommand = new Command(async (a) => await Sell(), (arg) => !_isSelling));

        public string Uuid { get; private set; }
        public double CurrentRateEur { get; private set; }
        public double CurrentProfitEur { get; private set; }

        public async override void Init(object initData)
        {
            base.Init(initData);

            _api = new MyntApi();
            _euroApi = new EuroApi();

            Task.Factory.StartNew(x => GetEuroPrice(), null);

            if (initData is Trade)
            {
                var trade = initData as Trade;

                Uuid = trade.Uuid;
                Currency = trade.Currency;
                Quantity = trade.Quantity;
                CurrentRate = trade.CurrentRate;
                CurrentProfitPercentage = trade.CurrentProfitPercentage;
                CurrentProfit = Quantity * trade.CurrentProfit;
                HasCurrentProfit = trade.HasCurrentProfit;
            }
        }

        private void ShowEuros()
        {
            ShowEuro = !ShowEuro;
        }

        private async Task GetEuroPrice()
        {
            var euroRate = await _euroApi.GetEuroPrice();
            _euroValue = euroRate.Eur.Sell;

            CurrentRateEur = CurrentRate * _euroValue;
            CurrentProfitEur = CurrentProfit * _euroValue;
        }

        public async Task Sell()
        {
            try
            {
                _isSelling = true;

                if (await _api.DirectSell(new Trade() { Uuid = this.Uuid }))
                {
                    await UserDialogs.Instance.AlertAsync("Sell order placed. Should be processed soon.", "Great success", "OK");
                    await CoreMethods.PopPageModel();
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync("Couldn't quick sell. Try again later.", "Sorry", "OK");
                }
            }
            catch (Exception xe)
            {
                await UserDialogs.Instance.AlertAsync("Couldn't quick sell. Try again later.", "Sorry", "OK");
                _isSelling = false;
            }
        }
    }
}
