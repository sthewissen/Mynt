using System;
using System.Collections.Generic;
using Mynt.Core.Models;
using System.Linq;

namespace Mynt.Core.Indicators
{

	public static partial class Extensions
	{
		public static List<decimal?> PivotHigh(this List<Candle> source, int barsLeft = 4, int barsRight = 2, bool fillNullValues = false)
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
					if (subSet[leftPivot].High > valueToCheck.High)
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
						if (subSet[rightPivot].High >= valueToCheck.High)
						{
							isPivot = false;
							break;
						}
					}

                    // If it's a pivot
					if (isPivot)
						result.Add(valueToCheck.High);
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
	}
}
