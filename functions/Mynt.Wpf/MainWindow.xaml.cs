using System;
using System.Windows;
using System.Windows.Threading;
using Mynt.Core.Bittrex;
using Mynt.Core.Strategies;
using Mynt.DataAccess.FileBasedStorage;
using Mynt.Services;

namespace Mynt.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };

        private SymbolInformationService symbolInformationService =
            new SymbolInformationService(new BittrexApi(), new JsonCandleProvider("Data"));

        public MainWindow()
        {
            InitializeComponent();

            timer.Tick += (s,e) => UpdateScreen();
            timer.Start();

            UpdateScreen();
        }

        private void UpdateScreen()
        {
            var historicalAdvicesModel = symbolInformationService.GetHistoricalAdvices(new[] { "btc-eth" }, new MacdSma());
            historicalAdvices.ItemsSource = historicalAdvicesModel;
        }
    }
}
