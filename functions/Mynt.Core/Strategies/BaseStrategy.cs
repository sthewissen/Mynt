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

        public DateTime GetSignalDate()
        {
            switch (IdealPeriod)
            {
                case Period.Minute:
                    return DateTime.UtcNow.AddMinutes(-2);
                case Period.FiveMinutes:
                    return DateTime.UtcNow.AddMinutes(-10);
                case Period.QuarterOfAnHour:
                    return DateTime.UtcNow.AddMinutes(-30);
                case Period.HalfAnHour:
                    return DateTime.UtcNow.AddHours(-1);
                case Period.Hour:
                    return DateTime.UtcNow.AddHours(-2);
                case Period.Day:
                    return DateTime.UtcNow.AddDays(-2);
                case Period.TwoHours:
                    return DateTime.UtcNow.AddHours(-4);
                case Period.FourHours:
                    return DateTime.UtcNow.AddHours(-8);
                default:
                    throw new ArgumentOutOfRangeException(nameof(IdealPeriod));
            }
        }

        public DateTime GetMinimumDateTime()
        {
            switch (IdealPeriod)
            {
                case Period.Minute:
                    return DateTime.UtcNow.AddMinutes(-MinimumAmountOfCandles);
                case Period.FiveMinutes:
                    return DateTime.UtcNow.AddMinutes(-(5 * MinimumAmountOfCandles));
                case Period.QuarterOfAnHour:
                    return DateTime.UtcNow.AddMinutes(-(15 * MinimumAmountOfCandles));
                case Period.HalfAnHour:
                    return DateTime.UtcNow.AddMinutes(-(30 * MinimumAmountOfCandles));
                case Period.Hour:
                    return DateTime.UtcNow.AddHours(-MinimumAmountOfCandles);
                case Period.Day:
                    return DateTime.UtcNow.AddDays(-MinimumAmountOfCandles);
                case Period.TwoHours:
                    return DateTime.UtcNow.AddHours(-(2 * MinimumAmountOfCandles));
                case Period.FourHours:
                    return DateTime.UtcNow.AddHours(-(4 * MinimumAmountOfCandles));
                default:
                    throw new ArgumentOutOfRangeException(nameof(IdealPeriod));
            }
        }

        public abstract List<ITradeAdvice> Prepare(List<Candle> candles);

        public abstract ITradeAdvice Forecast(List<Candle> candles);
    }
}
