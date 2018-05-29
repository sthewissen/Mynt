using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;
using System;
using System.Collections.Generic;

namespace Mynt.Core.Strategies
{
    public abstract class BaseStrategy : ITradingStrategy
    {
        public abstract string Name { get; }
        public abstract int MinimumAmountOfCandles { get; }
        public abstract Period IdealPeriod { get; }

        public DateTime GetCurrentCandleDateTime()
        {
            var minutes = IdealPeriod == Period.Minute ||
                IdealPeriod == Period.FiveMinutes ||
                IdealPeriod == Period.QuarterOfAnHour ||
                IdealPeriod == Period.HalfAnHour ? DateTime.UtcNow.Minute : 0;

            var hour = IdealPeriod == Period.Day ? 0 : DateTime.UtcNow.Hour;

            return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, hour, minutes, 0, 0, DateTimeKind.Utc);
        }

        public DateTime GetSignalDate()
        {
            var minutes = IdealPeriod == Period.Minute || IdealPeriod == Period.FiveMinutes || IdealPeriod == Period.QuarterOfAnHour || IdealPeriod == Period.HalfAnHour ? DateTime.UtcNow.Minute : 0;
            var hour = IdealPeriod == Period.Day ? 0 : DateTime.UtcNow.Hour;
            var current = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, hour, minutes, 0, 0, DateTimeKind.Utc);

            switch (IdealPeriod)
            {
                case Period.Minute:
                    return current.AddMinutes(-2);
                case Period.FiveMinutes:
                    return current.AddMinutes(-10);
                case Period.QuarterOfAnHour:
                    return current.AddMinutes(-30);
                case Period.HalfAnHour:
                    return current.AddHours(-1);
                case Period.Hour:
                    return current.AddHours(-2);
                case Period.Day:
                    return current.AddDays(-2);
                case Period.TwoHours:
                    return current.AddHours(-4);
                case Period.FourHours:
                    return current.AddHours(-8);
                default:
                    throw new ArgumentOutOfRangeException(nameof(IdealPeriod));
            }
        }

        public DateTime GetMinimumDateTime()
        {
            var hour = IdealPeriod == Period.Day ? 0 : DateTime.UtcNow.Hour;
            var minutes = IdealPeriod == Period.Minute || IdealPeriod == Period.FiveMinutes || IdealPeriod == Period.QuarterOfAnHour || IdealPeriod == Period.HalfAnHour ? DateTime.UtcNow.Minute : 0;
            var current = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, hour, minutes, 0, 0, DateTimeKind.Utc);

            switch (IdealPeriod)
            {
                case Period.Minute:
                    return current.AddMinutes(-MinimumAmountOfCandles);
                case Period.FiveMinutes:
                    return current.AddMinutes(-(5 * MinimumAmountOfCandles));
                case Period.QuarterOfAnHour:
                    return current.AddMinutes(-(15 * MinimumAmountOfCandles));
                case Period.HalfAnHour:
                    return current.AddMinutes(-(30 * MinimumAmountOfCandles));
                case Period.Hour:
                    return current.AddHours(-MinimumAmountOfCandles);
                case Period.Day:
                    return current.AddDays(-MinimumAmountOfCandles);
                case Period.TwoHours:
                    return current.AddHours(-(2 * MinimumAmountOfCandles));
                case Period.FourHours:
                    return current.AddHours(-(4 * MinimumAmountOfCandles));
                default:
                    throw new ArgumentOutOfRangeException(nameof(IdealPeriod));
            }
        }

        public abstract Candle GetSignalCandle(List<Candle> candles);

        public abstract List<TradeAdvice> Prepare(List<Candle> candles);

        public abstract TradeAdvice Forecast(List<Candle> candles);

    }
}
