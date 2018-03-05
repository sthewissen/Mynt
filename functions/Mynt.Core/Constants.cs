using System;
using System.Collections.Generic;
using Mynt.Core.Enums;

namespace Mynt.Core
{
    public class Constants
    {
        public const bool IsDryRunning = false;
        public const string BlockChainApiRoot = "https://blockchain.info";

        // Bittrex settings
        public const string BittrexApiKey = "";
        public const string BittrexApiSecret = "";

        // Binance settings
        public const string BinanceApiKey = "";
        public const string BinanceApiSecret = "";

        // Azure settings
        public const string TableStorageConnectionString = "";

        public const string OrderTableName = "orders";
        public const string TraderTableName = "traders";

        // Trader settings
        public const int MaxNumberOfConcurrentTrades = 2;
        public const double AmountOfBtcToInvestPerTrader = 0.01;
        public const double TransactionFeePercentage = 0.0025;

        // If we go below this profit percentage, we sell immediately.
        public const double StopLossPercentage = -0.07;

        // Use this to create a sell order as soon as the buy order is hit.
        // WARNING: This can't be used in combination with EnableTrailingStop.
        public const bool ImmediatelyPlaceSellOrder = false;
        public const double ImmediatelyPlaceSellOrderAtProfit = 0.02;

        // Use a trailing stop to lock in your profits.
        // WARNING: This can't be used in combination with ImmediatelyPlaceSellOrder.
        public const bool EnableTrailingStop = true;
        public const double TrailingStopStartingPercentage = 0.02;
        public const double TrailingStopPercentage = 0.05;

        // If set to true, orders that have not been bought for an entire cycle of the BuyTimer
        // are cancelled. This frees up a trader to look for other opportunities.
        public const bool CancelUnboughtOrdersEachCycle = true;

        // When enabled a first stop price is set to the current signal candle's low.
        public const bool PlaceFirstStopAtSignalCandleLow = false;

        // Setting this to 0 means we will not look at volume and only look at our AlwaysTradeList. 
        // Setting this to any value higher than 0 means we will get a list of markets currently
        // trading a volume above this value and analyze those for buy signals.
        public const int MinimumAmountOfVolume = 150;

        // Sets the bidding price. A value of 0.0 will use the ask price, 1.0 will use the last price and values between 
        // those interpolate between ask and last price. Using the ask price will guarantee quick success in bid, but
        // the bot will also end up paying more then would probably have been necessary.
        public const BuyInPriceStrategy BuyInPriceStrategy = Enums.BuyInPriceStrategy.SignalCandleClose;
        public const double AskLastBalance = 0.2;
        public const double BuyInPricePercentage = 0.002;

        // A list of duration and profit pairs. The duration is a value in minutes and the profit is a 
        // double containing a percentage. This list is used to define constraints such as
        // "Sell when 5 minutes have passed and profit is at 3%".
        public static readonly List<(int Duration, double Profit)> ReturnOnInvestment = new List<ValueTuple<int, double>>()
        {
           // new ValueTuple<int, double>(1440, 0.02),
        };

        // These are the markets we don't want to trade on
        public static readonly List<string> MarketBlackList = new List<string>() {
           "XVG", "TRX"
        };
        // These are the markets we want to trade on regardless of volume
        public static readonly List<string> AlwaysTradeList = new List<string>() {
           "VEN", "OMG", "NEO", "XRP", "LSK", "ETH", "LTC", "ARK"
        };
    }
}
