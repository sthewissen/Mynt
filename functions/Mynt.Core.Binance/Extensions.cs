using System;
using BinanceExchange.API.Enums;
using Mynt.Core.Enums;

namespace Mynt.Core.Binance
{
    public static class Extensions
    {
        public static BinanceExchange.API.Enums.OrderSide ToBinanceEquivalent(this Enums.OrderSide input)
        {
            switch (input)
            {
                case Enums.OrderSide.Buy:
                    return BinanceExchange.API.Enums.OrderSide.Buy;
                case Enums.OrderSide.Sell:
                    return BinanceExchange.API.Enums.OrderSide.Sell;
            }

            throw new ArgumentException($"{input} is an unknown OrderSide");
        }

        public static Enums.OrderSide ToCoreEquivalent(this BinanceExchange.API.Enums.OrderSide input)
        {
            switch (input)
            {
                case BinanceExchange.API.Enums.OrderSide.Buy:
                    return Enums.OrderSide.Buy;
                case BinanceExchange.API.Enums.OrderSide.Sell:
                    return Enums.OrderSide.Sell;
            }

            throw new ArgumentException($"{input} is an unknown OrderSide");
        }

        public static BinanceExchange.API.Enums.OrderStatus ToBinanceEquivalent(this Enums.OrderStatus input)
        {
            switch (input)
            {
                case Enums.OrderStatus.Canceled:
                    return BinanceExchange.API.Enums.OrderStatus.Cancelled;
                case Enums.OrderStatus.Expired:
                    return BinanceExchange.API.Enums.OrderStatus.Expired;
                case Enums.OrderStatus.Filled:
                    return BinanceExchange.API.Enums.OrderStatus.Filled;
                case Enums.OrderStatus.New:
                    return BinanceExchange.API.Enums.OrderStatus.New;
                case Enums.OrderStatus.PartiallyFilled:
                    return BinanceExchange.API.Enums.OrderStatus.PartiallyFilled;
                case Enums.OrderStatus.PendingCancel:
                    return BinanceExchange.API.Enums.OrderStatus.PendingCancel;
                case Enums.OrderStatus.Rejected:
                    return BinanceExchange.API.Enums.OrderStatus.Rejected;
            }

            throw new ArgumentException($"{input} is an unknown OrderStatus");
        }

        public static Enums.OrderStatus ToCoreEquivalent(this BinanceExchange.API.Enums.OrderStatus input)
        {
            switch (input)
            {
                case BinanceExchange.API.Enums.OrderStatus.Cancelled:
                    return Enums.OrderStatus.Canceled;
                case BinanceExchange.API.Enums.OrderStatus.Expired:
                    return Enums.OrderStatus.Expired;
                case BinanceExchange.API.Enums.OrderStatus.Filled:
                    return Enums.OrderStatus.Filled;
                case BinanceExchange.API.Enums.OrderStatus.New:
                    return Enums.OrderStatus.New;
                case BinanceExchange.API.Enums.OrderStatus.PartiallyFilled:
                    return Enums.OrderStatus.PartiallyFilled;
                case BinanceExchange.API.Enums.OrderStatus.PendingCancel:
                    return Enums.OrderStatus.PendingCancel;
                case BinanceExchange.API.Enums.OrderStatus.Rejected:
                    return Enums.OrderStatus.Rejected;
            }

            throw new ArgumentException($"{input} is an unknown OrderStatus");
        }

        public static BinanceExchange.API.Enums.TimeInForce ToBinanceEquivalent(this Enums.TimeInForce input)
        {
            switch (input)
            {
                case Enums.TimeInForce.GoodTilCanceled:
                    return BinanceExchange.API.Enums.TimeInForce.GTC;
                case Enums.TimeInForce.ImmediateOrCancel:
                    return BinanceExchange.API.Enums.TimeInForce.IOC;
            }

            throw new ArgumentException($"{input} is an unknown TimeInForce");
        }

        public static Enums.TimeInForce ToCoreEquivalent(this BinanceExchange.API.Enums.TimeInForce input)
        {
            switch (input)
            {
                case BinanceExchange.API.Enums.TimeInForce.GTC:
                    return Enums.TimeInForce.GoodTilCanceled;
                case BinanceExchange.API.Enums.TimeInForce.IOC:
                    return Enums.TimeInForce.ImmediateOrCancel;
            }

            throw new ArgumentException($"{input} is an unknown TimeInForce");
        }

        public static BinanceExchange.API.Enums.OrderType ToBinanceEquivalent(this Enums.OrderType input)
        {
            switch (input)
            {
                case Enums.OrderType.Limit:
                    return BinanceExchange.API.Enums.OrderType.Limit;
                case Enums.OrderType.LimitMaker:
                    return BinanceExchange.API.Enums.OrderType.LimitMaker;
                case Enums.OrderType.Market:
                    return BinanceExchange.API.Enums.OrderType.Market;
                case Enums.OrderType.StopLoss:
                    return BinanceExchange.API.Enums.OrderType.StopLoss;
                case Enums.OrderType.StopLossLimit:
                    return BinanceExchange.API.Enums.OrderType.StopLossLimit;
                case Enums.OrderType.TakeProfit:
                    return BinanceExchange.API.Enums.OrderType.TakeProfit;
                case Enums.OrderType.TakeProfitLimit:
                    return BinanceExchange.API.Enums.OrderType.TakeProfitLimit;
            }

            throw new ArgumentException($"{input} is an unknown OrderType");
        }

        public static Enums.OrderType ToCoreEquivalent(this BinanceExchange.API.Enums.OrderType input)
        {
            switch (input)
            {
                case BinanceExchange.API.Enums.OrderType.Limit:
                    return Enums.OrderType.Limit;
                case BinanceExchange.API.Enums.OrderType.LimitMaker:
                    return Enums.OrderType.LimitMaker;
                case BinanceExchange.API.Enums.OrderType.Market:
                    return Enums.OrderType.Market;
                case BinanceExchange.API.Enums.OrderType.StopLoss:
                    return Enums.OrderType.StopLoss;
                case BinanceExchange.API.Enums.OrderType.StopLossLimit:
                    return Enums.OrderType.StopLossLimit;
                case BinanceExchange.API.Enums.OrderType.TakeProfit:
                    return Enums.OrderType.TakeProfit;
                case BinanceExchange.API.Enums.OrderType.TakeProfitLimit:
                    return Enums.OrderType.TakeProfitLimit;
            }

            throw new ArgumentException($"{input} is an unknown OrderType");
        }
    }
}
