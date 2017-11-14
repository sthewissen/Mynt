using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Mynt.Pages
{
    public partial class CurrentTradePage : ContentPage
    {
        public CurrentTradePage()
        {
            InitializeComponent();

            ListViewTrades.ItemSelected += (sender, e) => { ListViewTrades.SelectedItem = null; };
        }
    }
}
