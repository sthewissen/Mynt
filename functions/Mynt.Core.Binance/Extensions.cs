using System;
using Binance.Net.Objects;
using BinanceNet = Binance.Net;
using Mynt.Core.Enums;

namespace Mynt.Core.Binance
{
    public static class Extensions
    {
        public static BinanceNet.Objects.OrderSide ToBinanceEquivalent(this Enums.OrderSide input)
        {
            switch (input)
            {
                case Enums.OrderSide.Buy:
                    return BinanceNet.Objects.OrderSide.Buy;
                case Enums.OrderSide.Sell:
                    return BinanceNet.Objects.OrderSide.Sell;
            }

            throw new ArgumentException($"{input} is an unknown OrderSide");
        }

        public static Enums.OrderSide ToCoreEquivalent(this BinanceNet.Objects.OrderSide input)
        {
            switch (input)
            {
                case BinanceNet.Objects.OrderSide.Buy:
                    return Enums.OrderSide.Buy;
                case BinanceNet.Objects.OrderSide.Sell:
                    return Enums.OrderSide.Sell;
            }

            throw new ArgumentException($"{input} is an unknown OrderSide");
        }

        public static BinanceNet.Objects.OrderStatus ToBinanceEquivalent(this Enums.OrderStatus input)
        {
            switch (input)
            {
                case Enums.OrderStatus.Canceled:
                    return BinanceNet.Objects.OrderStatus.Canceled;
                case Enums.OrderStatus.Expired:
                    return BinanceNet.Objects.OrderStatus.Expired;
                case Enums.OrderStatus.Filled:
                    return BinanceNet.Objects.OrderStatus.Filled;
                case Enums.OrderStatus.New:
                    return BinanceNet.Objects.OrderStatus.New;
                case Enums.OrderStatus.PartiallyFilled:
                    return BinanceNet.Objects.OrderStatus.PartiallyFilled;
                case Enums.OrderStatus.PendingCancel:
                    return BinanceNet.Objects.OrderStatus.PendingCancel;
                case Enums.OrderStatus.Rejected:
                    return BinanceNet.Objects.OrderStatus.Rejected;
            }

            throw new ArgumentException($"{input} is an unknown OrderStatus");
        }

        public static Enums.OrderStatus ToCoreEquivalent(this BinanceNet.Objects.OrderStatus input)
        {
            switch (input)
            {
                case BinanceNet.Objects.OrderStatus.Canceled:
                    return Enums.OrderStatus.Canceled;
                case BinanceNet.Objects.OrderStatus.Expired:
                    return Enums.OrderStatus.Expired;
                case BinanceNet.Objects.OrderStatus.Filled:
                    return Enums.OrderStatus.Filled;
                case BinanceNet.Objects.OrderStatus.New:
                    return Enums.OrderStatus.New;
                case BinanceNet.Objects.OrderStatus.PartiallyFilled:
                    return Enums.OrderStatus.PartiallyFilled;
                case BinanceNet.Objects.OrderStatus.PendingCancel:
                    return Enums.OrderStatus.PendingCancel;
                case BinanceNet.Objects.OrderStatus.Rejected:
                    return Enums.OrderStatus.Rejected;
            }

            throw new ArgumentException($"{input} is an unknown OrderStatus");
        }

        public static BinanceNet.Objects.TimeInForce ToBinanceEquivalent(this Enums.TimeInForce input)
        {
            switch (input)
            {
                case Enums.TimeInForce.GoodTilCanceled:
                    return BinanceNet.Objects.TimeInForce.GoodTillCancel;
                case Enums.TimeInForce.ImmediateOrCancel:
                    return BinanceNet.Objects.TimeInForce.ImmediateOrCancel;
            }

            throw new ArgumentException($"{input} is an unknown TimeInForce");
        }

        public static Enums.TimeInForce ToCoreEquivalent(this BinanceNet.Objects.TimeInForce input)
        {
            switch (input)
            {
                case BinanceNet.Objects.TimeInForce.GoodTillCancel:
                    return Enums.TimeInForce.GoodTilCanceled;
                case BinanceNet.Objects.TimeInForce.ImmediateOrCancel:
                    return Enums.TimeInForce.ImmediateOrCancel;
            }

            throw new ArgumentException($"{input} is an unknown TimeInForce");
        }

        public static Core.Enums.Period ToCoreEquivalent(this KlineInterval period)
        {
            switch (period)
            {
                case KlineInterval.OneDay:
                    return Core.Enums.Period.Day;
                case KlineInterval.FiveMinutes:
                    return Core.Enums.Period.FiveMinutes;
                case KlineInterval.OneHour:
                    return Core.Enums.Period.Hour;
                case KlineInterval.OneMinute:
                    return Core.Enums.Period.Minute;
                case KlineInterval.ThirtyMinutes:
                    return Core.Enums.Period.HalfAnHour;
                case KlineInterval.FourHour:
                    return Core.Enums.Period.FourHours;
                case KlineInterval.TwoHour:
                    return Core.Enums.Period.TwoHours;
                case KlineInterval.FiveteenMinutes:
                    return Core.Enums.Period.QuarterOfAnHour;
                default:
                    throw new ArgumentException($"{period} is an unknown value for Period");
            }
        }

        public static KlineInterval ToBinanceEquivalent(this Core.Enums.Period period)
        {
            switch (period)
            {
                case Core.Enums.Period.Day:
                    return KlineInterval.OneDay;
                case Core.Enums.Period.FiveMinutes:
                    return KlineInterval.FiveMinutes;
                case Core.Enums.Period.HalfAnHour:
                    return KlineInterval.ThirtyMinutes;
                case Core.Enums.Period.Hour:
                    return KlineInterval.OneHour;
                case Core.Enums.Period.Minute:
                    return KlineInterval.OneMinute;
                case Core.Enums.Period.FourHours:
                    return KlineInterval.FourHour;
                case Core.Enums.Period.QuarterOfAnHour:
                    return KlineInterval.FiveteenMinutes;
                case Core.Enums.Period.TwoHours:
                    return KlineInterval.TwoHour;
                default:
                    throw new ArgumentException($"{period} is an unknown value for Period");
            }
        }

        public static BinanceNet.Objects.OrderType ToBinanceEquivalent(this Enums.OrderType input)
        {
            switch (input)
            {
                case Enums.OrderType.Limit:
                    return BinanceNet.Objects.OrderType.Limit;
                case Enums.OrderType.LimitMaker:
                    return BinanceNet.Objects.OrderType.LimitMaker;
                case Enums.OrderType.Market:
                    return BinanceNet.Objects.OrderType.Market;
                case Enums.OrderType.StopLoss:
                    return BinanceNet.Objects.OrderType.StopLoss;
                case Enums.OrderType.StopLossLimit:
                    return BinanceNet.Objects.OrderType.StopLossLimit;
                case Enums.OrderType.TakeProfit:
                    return BinanceNet.Objects.OrderType.TakeProfit;
                case Enums.OrderType.TakeProfitLimit:
                    return BinanceNet.Objects.OrderType.TakeProfitLimit;
            }

            throw new ArgumentException($"{input} is an unknown OrderType");
        }

        public static Enums.OrderType ToCoreEquivalent(this BinanceNet.Objects.OrderType input)
        {
            switch (input)
            {
                case BinanceNet.Objects.OrderType.Limit:
                    return Enums.OrderType.Limit;
                case BinanceNet.Objects.OrderType.LimitMaker:
                    return Enums.OrderType.LimitMaker;
                case BinanceNet.Objects.OrderType.Market:
                    return Enums.OrderType.Market;
                case BinanceNet.Objects.OrderType.StopLoss:
                    return Enums.OrderType.StopLoss;
                case BinanceNet.Objects.OrderType.StopLossLimit:
                    return Enums.OrderType.StopLossLimit;
                case BinanceNet.Objects.OrderType.TakeProfit:
                    return Enums.OrderType.TakeProfit;
                case BinanceNet.Objects.OrderType.TakeProfitLimit:
                    return Enums.OrderType.TakeProfitLimit;
            }

            throw new ArgumentException($"{input} is an unknown OrderType");
        }
    }
}
