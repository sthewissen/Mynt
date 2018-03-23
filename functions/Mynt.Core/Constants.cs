using System;
using System.Collections.Generic;
using Mynt.Core.Enums;

namespace Mynt.Core
{
    public abstract class BaseSettings
    {
        protected static void TrySetFromConfig(Action action)
        {
            try
            {
                action();
            }
            catch
            {
            }
        }
    }

    public class Constants : BaseSettings
    {
        public Constants()
        {
            TrySetFromConfig(() => BinanceApiKey = AppSettings.Get<string>(nameof(BinanceApiKey)));
            TrySetFromConfig(() => BinanceApiSecret = AppSettings.Get<string>(nameof(BinanceApiSecret)));

            TrySetFromConfig(() => BittrexApiKey = AppSettings.Get<string>(nameof(BittrexApiKey)));
            TrySetFromConfig(() => BittrexApiSecret = AppSettings.Get<string>(nameof(BittrexApiSecret)));

            TrySetFromConfig(() => IsDryRunning = AppSettings.Get<bool>(nameof(IsDryRunning)));
            TrySetFromConfig(() => TableStorageConnectionString = AppSettings.Get<string>(nameof(TableStorageConnectionString)));
            TrySetFromConfig(() => OrderTableName = AppSettings.Get<string>(nameof(OrderTableName)));
            TrySetFromConfig(() => TraderTableName = AppSettings.Get<string>(nameof(TraderTableName)));
        }

        public bool IsDryRunning { get; set; } = false;

        // Bittrex settings
        public string BittrexApiKey { get; set; } = "";
        public string BittrexApiSecret { get; set; } = "";

        // Binance settings
        public string BinanceApiKey { get; set; } = "";
        public string BinanceApiSecret { get; set; } = "";

        // Azure settings
        public string TableStorageConnectionString { get; set; } = "";

        public string OrderTableName { get; set; } = "orders";
        public string TraderTableName { get; set; } = "traders";

        // Trader settings
        public int MaxNumberOfConcurrentTrades { get; set; } = 2;
        public double AmountOfBtcToInvestPerTrader { get; set; } = 0.01;
        public double TransactionFeePercentage { get; set; } = 0.0025;

        // If we go below this profit percentage, we sell immediately.
        public double StopLossPercentage { get; set; } = -0.07;

        // Use this to create a sell order as soon as the buy order is hit.
        // WARNING: This can't be used in combination with EnableTrailingStop.
        public bool ImmediatelyPlaceSellOrder { get; set; } = false;
        public double ImmediatelyPlaceSellOrderAtProfit { get; set; } = 0.02;

        // Use a trailing stop to lock in your profits.
        // WARNING: This can't be used in combination with ImmediatelyPlaceSellOrder.
        public bool EnableTrailingStop { get; set; } = true;
        public double TrailingStopStartingPercentage { get; set; } = 0.02;
        public double TrailingStopPercentage { get; set; } = 0.05;

        // If set to true, orders that have not been bought for an entire cycle of the BuyTimer
        // are cancelled. This frees up a trader to look for other opportunities.
        public bool CancelUnboughtOrdersEachCycle { get; set; } = true;

        // When enabled a first stop price is set to the current signal candle's low.
        public bool PlaceFirstStopAtSignalCandleLow { get; set; } = false;

        // Setting this to 0 means we will not look at volume and only look at our AlwaysTradeList. 
        // Setting this to any value higher than 0 means we will get a list of markets currently
        // trading a volume above this value and analyze those for buy signals.
        public int MinimumAmountOfVolume { get; set; } = 150;

        // Sets the bidding price. A value of 0.0 will use the ask price, 1.0 will use the last price and values between 
        // those interpolate between ask and last price. Using the ask price will guarantee quick success in bid, but
        // the bot will also end up paying more then would probably have been necessary.
        public BuyInPriceStrategy BuyInPriceStrategy { get; set; } = Enums.BuyInPriceStrategy.SignalCandleClose;
        public double AskLastBalance { get; set; } = 0.2;
        public double BuyInPricePercentage { get; set; } = 0.002;

        // A list of duration and profit pairs. The duration is a value in minutes and the profit is a 
        // double containing a percentage. This list is used to define constraints such as
        // "Sell when 5 minutes have passed and profit is at 3%".
        public List<(int Duration, double Profit)> ReturnOnInvestment { get; set; } = new List<ValueTuple<int, double>>()
        {
            // new ValueTuple<int, double>(1440, 0.02),
        };

        // These are the markets we don't want to trade on
        public List<string> MarketBlackList { get; set; } = new List<string>() {
           "XVG", "TRX"
        };

        // These are the markets we want to trade on regardless of volume
        public List<string> OnlyTradeList { get; set; } = new List<string>() {
           "VEN"
        };

        // These are the markets we want to trade on regardless of volume
        public List<string> AlwaysTradeList { get; set; } = new List<string>() {
           "VEN", "OMG", "NEO", "XRP", "LSK", "ETH", "LTC", "ARK"
        };
    }
}
