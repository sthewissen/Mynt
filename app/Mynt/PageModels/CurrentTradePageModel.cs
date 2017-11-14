using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using FreshMvvm;
using Mynt.Helpers;
using Mynt.Models;
using Mynt.Services;
using PropertyChanged;
using Xamarin.Forms;

namespace Mynt.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class CurrentTradePageModel : FreshBasePageModel
    {
        private MyntApi _api;
        private bool _openingPage;

        public bool HasData => Items.Count > 0;
        public bool IsRefreshing { get; set; }
        public bool IsLoading { get; set; }

        [AlsoNotifyFor(nameof(HasData))]
        public List<Trade> Items { get; set; }

        ICommand itemSelectedCommand;
        public ICommand ItemSelectedCommand => itemSelectedCommand ?? (itemSelectedCommand = new Command<Trade>(async (a) => await Sell(a), (arg) => !_openingPage));

        ICommand refreshCommand;
        public ICommand RefreshCommand => refreshCommand ?? (refreshCommand = new Command(async (a) => await RefreshData(true, false)));

        ICommand loadCommand;
        public ICommand LoadCommand => loadCommand ?? (loadCommand = new Command(async (a) => await RefreshData(false, true)));

        public override void Init(object initData)
        {
            base.Init(initData);
            Items = new List<Trade>();
            _api = new MyntApi();

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
                var items = await _api.GetActiveTrades();
                Items = items;
            }

            if (refresh)
                IsRefreshing = false;
            else
                IsLoading = false;
        }

        private async Task Sell(Trade a)
        {
            _openingPage = true;
            await CoreMethods.PushPageModel<TradePageModel>(a, false, true);
            _openingPage = false;
        }
    }
}
