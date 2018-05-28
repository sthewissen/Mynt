using Mynt.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription<T>(this T enumerationValue) where T : struct
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            //If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString();
        }

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

        public static Period FromMinutesEquivalent(this int period)
        {
            switch (period)
            {
                case 1:
                    return Period.Minute;
                case 5:
                    return Period.FiveMinutes;
                case 15:
                    return Period.QuarterOfAnHour;
                case 30:
                    return Period.HalfAnHour;
                case 60:
                    return Period.Hour;
                case 120:
                    return Period.TwoHours;
                case 240:
                    return Period.FourHours;
                case 1440:
                    return Period.Day;
                default:
                    throw new ArgumentException($"{period} is an unknown value for Period");
            }
        }
    }
}
