using System;
using System.Collections.Generic;
using Mynt.Core.Models;
using System.Linq;

namespace Mynt.Core.Indicators
{

	public static partial class Extensions
	{
		public static List<decimal?> PivotLow(this List<Candle> source, int barsLeft = 4, int barsRight = 2, bool fillNullValues = false)
		{
			var result = new List<decimal?>();

			for (int i = 0; i < source.Count; i++)
			{
				if (i < barsLeft + barsRight)
				{
					result.Add(null);
					continue;
				}

				var isPivot = true;
				var subSet = source.Skip(i - barsLeft - barsRight).Take(barsLeft + barsRight + 1).ToList();
				var valueToCheck = subSet[barsLeft];

				// Check if the [barsLeft] bars left of what we're checking all have lower highs or equal
				for (int leftPivot = 0; leftPivot < barsLeft; leftPivot++)
				{
					if (subSet[leftPivot].Low < valueToCheck.Low)
					{
						isPivot = false;
						break;
					}
				}

				// If it's still a pivot by this point, carry on checking the right side...
				if (isPivot)
				{
					// If the [barsRight] right side all have lower highs, it's a pivot!
					for (int rightPivot = barsLeft + 1; rightPivot < subSet.Count; rightPivot++)
					{
						if (subSet[rightPivot].Low <= valueToCheck.Low)
						{
							isPivot = false;
							break;
						}
					}

					// If it's a pivot
					if (isPivot)
						result.Add(valueToCheck.Low);
					else
						result.Add(null);
				}
				else
				{
					result.Add(null);
				}
			}

			if (fillNullValues)
			{
				return FillPivotNulls(result);
			}

			return result;
		}

		static List<decimal?> FillPivotNulls(List<decimal?> result)
		{
			var values = new List<Tuple<decimal, int>>();
			var nullCounter = 0;

			foreach (var item in result)
			{
				if (item.HasValue)
				{
					values.Add(new Tuple<decimal, int>(item.Value, nullCounter));
					nullCounter = 0;
				}
				else
				{
					nullCounter += 1;
				}
			}

			var finalList = new List<decimal?>();
			var isFirst = true;

			for (int i = 0; i < values.Count; i++)
			{
				if (isFirst)
				{
					for (int j = 0; j < values[i].Item2; j++)
						finalList.Add(null);

					finalList.Add(values[i].Item1);

					isFirst = false;
				}
				else
				{
					var current = values[i];
					var previous = values[i - 1];
					var count = current.Item2;

					for (int x = 1; x <= count; x++)
					{
						if (current.Item1 > previous.Item1)
						{
							var amountToUse = (current.Item1 - previous.Item1) / (count + 1);
							finalList.Add(Math.Round(previous.Item1 + (amountToUse * x), 8));
						}
						else
						{
							var amountToUse = (previous.Item1 - current.Item1) / (count + 1);
							finalList.Add(Math.Round(previous.Item1 - (amountToUse * x), 8));
						}
					}

					finalList.Add(current.Item1);
				}
			}

			var finalCount = finalList.Count;
			for (int i = 0; i < result.Count - finalCount; i++)
			{
				finalList.Add(null);
			}

			return finalList;
		}
	}
}
