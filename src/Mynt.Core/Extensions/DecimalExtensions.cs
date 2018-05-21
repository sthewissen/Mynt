using System;
using System.Collections.Generic;
using System.Linq;

namespace Mynt.Core.Extensions
{
	public static class DecimalExtensions
	{
		public static List<decimal> Lowest(this List<decimal> source, int length)
		{
			var result = new List<decimal>();

			for (int i = 1; i <= source.Count; i++)
			{
				if (i < length)
				{
					result.Add(source.Take(i).Min());
				}
				else
				{
					result.Add(source.Skip(i - length).Take(length).Min());
				}
			}

			return result;
		}

		public static List<decimal?> Lowest(this List<decimal?> source, int length)
		{
			var result = new List<decimal?>();

			for (int i = 1; i <= source.Count; i++)
			{
				if (i < length)
				{
					result.Add(source.Take(i).Min());
				}
				else
				{
					result.Add(source.Skip(i - length).Take(length).Min());
				}
			}

			return result;
		}

		public static List<decimal> Avg(this List<decimal> source, int length)
		{
			var result = new List<decimal>();

			for (int i = 1; i <= source.Count; i++)
			{
				if (i < length)
				{
					result.Add(source.Take(i).Average());
				}
				else
				{
					result.Add(source.Skip(i - length).Take(length).Average());
				}
			}

			return result;
		}

		public static List<decimal?> Avg(this List<decimal?> source, int length)
		{
			var result = new List<decimal?>();

			for (int i = 1; i <= source.Count; i++)
			{
				if (i < length)
				{
					result.Add(source.Take(i).Average());
				}
				else
				{
					result.Add(source.Skip(i - length).Take(length).Average());
				}
			}

			return result;
		}

		public static List<decimal> Highest(this List<decimal> source, int length)
		{
			var result = new List<decimal>();

			for (int i = 1; i <= source.Count; i++)
			{
				if (i < length)
				{
					result.Add(source.Take(i).Max());
				}
				else
				{
					result.Add(source.Skip(i - length).Take(length).Max());
				}
			}

			return result;
		}

		public static List<decimal?> Highest(this List<decimal?> source, int length)
		{
			var result = new List<decimal?>();

			for (int i = 1; i <= source.Count; i++)
			{
				if (i < length)
				{
					result.Add(source.Take(i).Max());
				}
				else
				{
					result.Add(source.Skip(i - length).Take(length).Max());
				}
			}

			return result;
		}

		#region crossunders

		public static List<bool> Crossunder(this List<decimal?> source, decimal value)
		{
			var result = new List<bool>();

			for (int i = 0; i < source.Count; i++)
			{
				if (i == 0)
					result.Add(false);
				else
				{
					result.Add(source[i] < value && source[i - 1] >= value);
				}
			}

			return result;
		}
        
		public static List<bool> Crossunder(this List<decimal?> source, List<decimal?> value)
		{
			var result = new List<bool>();

			for (int i = 0; i < source.Count; i++)
			{
				if (i == 0)
					result.Add(false);
				else
				{
					result.Add(source[i] < value[i] && source[i - 1] >= value[i - 1]);
				}
			}

			return result;
		}

		public static List<bool> Crossunder(this List<decimal> source, decimal value)
		{
			var result = new List<bool>();

			for (int i = 0; i < source.Count; i++)
			{
				if (i == 0)
					result.Add(false);
				else
				{
					result.Add(source[i] < value && source[i - 1] >= value);
				}
			}

			return result;
		}
        
		public static List<bool> Crossunder(this List<decimal> source, List<decimal?> value)
		{
			var result = new List<bool>();

			for (int i = 0; i < source.Count; i++)
			{
				if (i == 0)
					result.Add(false);
				else
				{
					result.Add(source[i] < value[i] && source[i - 1] >= value[i - 1]);
				}
			}

			return result;
		}
        
        public static List<bool> Crossunder(this List<decimal?> source, List<decimal> value)
        {
            var result = new List<bool>();

            for (int i = 0; i < source.Count; i++)
            {
                if (i == 0)
                    result.Add(false);
                else
                {
                    result.Add(source[i] < value[i] && source[i - 1] >= value[i - 1]);
                }
            }

            return result;
        }

		#endregion

		#region crossovers

		public static List<bool> Crossover(this List<decimal?> source, decimal value)
		{
			var result = new List<bool>();

			for (int i = 0; i < source.Count; i++)
			{
				if (i == 0)
					result.Add(false);
				else
				{
					result.Add(source[i] > value && source[i - 1] <= value);
				}
			}

			return result;
		}


		public static List<bool> Crossover(this List<decimal?> source, List<decimal?> value)
		{
			var result = new List<bool>();

			for (int i = 0; i < source.Count; i++)
			{
				if (i == 0)
					result.Add(false);
				else
				{
					result.Add(source[i] > value[i] && source[i - 1] <= value[i - 1]);
				}
			}

			return result;
		}


		public static List<bool> Crossover(this List<decimal> source, decimal value)
		{
			var result = new List<bool>();

			for (int i = 0; i < source.Count; i++)
			{
				if (i == 0)
					result.Add(false);
				else
				{
					result.Add(source[i] > value && source[i - 1] <= value);
				}
			}

			return result;
		}


		public static List<bool> Crossover(this List<decimal> source, List<decimal?> value)
		{
			var result = new List<bool>();

			for (int i = 0; i < source.Count; i++)
			{
				if (i == 0)
					result.Add(false);
				else
				{
					result.Add(source[i] > value[i] && source[i - 1] <= value[i - 1]);
				}
			}

			return result;
		}

		#endregion
	}
}
