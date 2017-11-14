using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using FreshMvvm;
using Mynt.Helpers;
using Mynt.Models;
using Mynt.Services;
using PropertyChanged;
using Xamarin.Forms;

namespace Mynt.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class HistoryPageModel : FreshBasePageModel
    {
        private MyntApi _api;
        private EuroApi _euroApi;
        private double _euroValue;

        public bool HasData => Items.Count > 0;
        public bool IsRefreshing { get; set; }
        public bool IsLoading { get; set; }

        [AlsoNotifyFor(nameof(HasData))]
        public List<Trade> Items { get; set; }

        public double TotalProfit { get; set; }
        public double TodaysProfit { get; set; }
        public bool HasTotalProfit { get; set; }
        public bool HasTodayProfit { get; set; }
        public bool ShowEuro { get; set; }
        public double OverallBalance { get; set; }

        ICommand refreshCommand;
        public ICommand RefreshCommand => refreshCommand ?? (refreshCommand = new Command(async (a) => await RefreshData(true, true)));

        ICommand showEuroCommand;
        public ICommand ShowEuroCommand => showEuroCommand ?? (showEuroCommand = new Command(() => ShowEuros()));

        ICommand loadCommand;
        public ICommand LoadCommand => loadCommand ?? (loadCommand = new Command(async (a) => await RefreshData(false, true)));

        public double TotalProfitEur { get; private set; }
        public double TodaysProfitEur { get; private set; }
        public double OverallBalanceEur { get; private set; }

        public override void Init(object initData)
        {
            base.Init(initData);
            Items = new List<Trade>();

            _api = new MyntApi();
            _euroApi = new EuroApi();

            Task.Factory.StartNew(x => RefreshData(false), null);
        }

        private async Task RefreshData(bool refresh = true, bool message = false)
        {
            if (refresh)
                IsRefreshing = true;
            else
                IsLoading = true;

            if (Settings.FunctionRoot != string.Empty)
            {
                var history = await _api.GetHistoricTrades();

                Items = history.Trades;

                TotalProfit = history.TotalProfit;
                TodaysProfit = history.TodaysProfit;

                HasTotalProfit = history.HasTotalProfit;
                HasTodayProfit = history.HasTodayProfit;
                OverallBalance = history.OverallBalance;

                await GetEuroPrice();
            }

            if (refresh)
                IsRefreshing = false;
            else
                IsLoading = false;
        }

        private async Task GetEuroPrice()
        {
            try
            {
                var euroRate = await _euroApi.GetEuroPrice();
                _euroValue = euroRate.Eur.Sell;

                TotalProfitEur = TotalProfit * _euroValue;
                TodaysProfitEur = TodaysProfit * _euroValue;
                OverallBalanceEur = OverallBalance * _euroValue;
            }
            catch (Exception x)
            {

            }
        }

        private void ShowEuros()
        {
            ShowEuro = !ShowEuro;
        }
    }
}
