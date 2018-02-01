using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Bittrex.Models;
using Mynt.Core.Enums;

namespace Mynt.Core.Bittrex
{
    public static class Extensions
    {
        public static List<Core.Models.Candle> ToGenericCandles(this List<Candle> candles)
        {
            return candles.Select(x => new Core.Models.Candle
            {
                Close = x.C,
                High = x.H,
                Low = x.L,
                Open = x.O,
                Timestamp = x.T,
                Volume = x.V
            }).ToList();
        }

        public static Core.Models.Period ToCoreEquivalent(this Period period)
        {
            switch (period)
            {
                case Period.Day:
                    return Core.Models.Period.Day;
                case Period.FiveMin:
                    return Core.Models.Period.FiveMinutes;
                case Period.Hour:
                    return Core.Models.Period.Hour;
                case Period.OneMin:
                    return Core.Models.Period.Minute;
                case Period.ThirtyMin:
                    return Core.Models.Period.HalfAnHour;
                default:
                    throw new ArgumentException($"{period} is an unknown value for Period");
            }             
        }
        
        public static Period ToBittrexEquivalent(this Core.Models.Period period)
        {
            switch (period)
            {
                case Core.Models.Period.Day:
                    return Period.Day;
                case Core.Models.Period.FiveMinutes:
                    return Period.FiveMin;
                case Core.Models.Period.HalfAnHour:
                    return Period.ThirtyMin;
                case Core.Models.Period.Hour:
                    return Period.Hour;
                case Core.Models.Period.Minute:
                    return Period.OneMin;
                case Core.Models.Period.FourHours:
                case Core.Models.Period.QuarterOfAnHour:
                case Core.Models.Period.TwoHours:
                    throw new ArgumentException($"{period} has no mapping");
                default:
                    throw new ArgumentException($"{period} is an unknown value for Period");
            }
        }

        public static OrderSide ToOrderSide(this string input)
        {
            if (input.ToLower().Contains("buy"))
            {
                return OrderSide.Buy;
            }
            if (input.ToLower().Contains("sell"))
            {
                return OrderSide.Sell;
            }

            throw new ArgumentException("'{input}' cannot be mapped an OrderSide");
        }

        public static OrderType ToOrderType(this string input)
        {
            if (input.Equals("BUY", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("SELL", StringComparison.OrdinalIgnoreCase))
            {
                return OrderType.Market;
            }
            if (input.ToLower().Contains("limit"))
            {
                return OrderType.Limit;
            }

            throw new ArgumentException("'{input}' cannot be mapped an OrderType");
        }

    }
}
