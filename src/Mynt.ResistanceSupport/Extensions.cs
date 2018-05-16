using System;
namespace Mynt.ResistanceSupport
{
    public static class Extensions
    {
		public static bool IsWithin(this decimal value, decimal a, decimal b)
        {
			if (a > b)
				return value >= b && value <= a;
			else
				return value >= a && value <= b;
        }
    }
}
