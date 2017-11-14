using System;
using Xamarin.Forms;

namespace Mynt.Behaviors
{
    public class ListViewNoSelectionBehavior : Behavior<ListView>
    {
        protected override void OnAttachedTo(ListView listview)
        {
            listview.ItemSelected += OnItemSelected;
            base.OnAttachedTo(listview);
        }

        protected override void OnDetachingFrom(ListView listview)
        {
            listview.ItemSelected -= OnItemSelected;
            base.OnDetachingFrom(listview);
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}