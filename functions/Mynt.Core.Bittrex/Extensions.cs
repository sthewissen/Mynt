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

        public static Core.Enums.Period ToCoreEquivalent(this Bittrex.Models.Period period)
        {
            switch (period)
            {
                case Bittrex.Models.Period.Day:
                    return Core.Enums.Period.Day;
                case Bittrex.Models.Period.FiveMin:
                    return Core.Enums.Period.FiveMinutes;
                case Bittrex.Models.Period.Hour:
                    return Core.Enums.Period.Hour;
                case Bittrex.Models.Period.OneMin:
                    return Core.Enums.Period.Minute;
                case Bittrex.Models.Period.ThirtyMin:
                    return Core.Enums.Period.HalfAnHour;
                default:
                    throw new ArgumentException($"{period} is an unknown value for Period");
            }             
        }
        
        public static Bittrex.Models.Period ToBittrexEquivalent(this Core.Enums.Period period)
        {
            switch (period)
            {
                case Core.Enums.Period.Day:
                    return Bittrex.Models.Period.Day;
                case Core.Enums.Period.FiveMinutes:
                    return Bittrex.Models.Period.FiveMin;
                case Core.Enums.Period.HalfAnHour:
                    return Bittrex.Models.Period.ThirtyMin;
                case Core.Enums.Period.Hour:
                    return Bittrex.Models.Period.Hour;
                case Core.Enums.Period.Minute:
                    return Bittrex.Models.Period.OneMin;
                case Core.Enums.Period.FourHours:
                case Core.Enums.Period.QuarterOfAnHour:
                case Core.Enums.Period.TwoHours:
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
