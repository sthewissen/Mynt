using Mynt.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Extensions
{
    public static class EnumExtensions
    {
        public static int ToMinutesEquivalent(this Period period)
        {
            switch (period)
            {
                case Period.Minute:
                    return 1;
                case Period.FiveMinutes:
                    return 5;
                case Period.QuarterOfAnHour:
                    return 15;
                case Period.HalfAnHour:
                    return 30;
                case Period.Hour:
                    return 60;
                case Period.TwoHours:
                    return 120;
                case Period.FourHours:
                    return 240;
                case Period.Day:
                    return 1440;
                default:
                    throw new ArgumentException($"{period} is an unknown value for Period");
            }
        }
    }
}
