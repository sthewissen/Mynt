using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Extensions;
using Mynt.Core.Models;

namespace Mynt.Core.Indicators
{
	public enum CandlePattern
	{
		Doji,
		ShootingStar,
		BearishEveningStar,
		BullishMorningStar,
		BullishHammer,
		BearishInvertedHammer,
		BearishHarami,
		BullishHarami,
		BearishEngulfing,
		BullishEngulfing,
		PiercingLine,
		BullishBelt,
		BullishKicker,
		BearishKicker,
		BearishHangingMan,
		BearishDarkCloudCover,
		BullishMarubozu,
		BearishMarubozu
	}

	public static partial class Extensions
	{
		public static List<CandlePattern?> CandlePatterns(this List<Candle> source, decimal dojiSize = 0.05m)
		{
			var result = new List<CandlePattern?>();

			var open = source.Open();
			var close = source.Close();
			var high = source.High();
			var low = source.Low();

			for (int i = 0; i < source.Count; i++)
			{
				if ((Math.Abs(open[i] - close[i]) <= (high[i] - low[i]) * dojiSize))
				{
					result.Add(CandlePattern.Doji);
					continue;
				}

				if (((high[i] - low[i]) > 3 * (open[i] - close[i])) && ((close[i] - low[i]) / (.001m + high[i] - low[i]) > 0.6m) && ((open[i] - low[i]) / (.001m + high[i] - low[i]) > 0.6m))
				{
					result.Add(CandlePattern.BullishHammer);
					continue;
				}

				if (((high[i] - low[i]) > 3 * (open[i] - close[i])) && ((high[i] - close[i]) / (.001m + high[i] - low[i]) > 0.6m) && ((high[i] - open[i]) / (.001m + high[i] - low[i]) > 0.6m))
				{
					result.Add(CandlePattern.BearishInvertedHammer);
					continue;
				}

				// These patterns require at least 2 data points
				if (i > 0)
				{
					if (open[i - 1] < close[i - 1] && open[i] > close[i - 1] && high[i] - Math.Max(open[i], close[i]) >= Math.Abs(open[i] - close[i]) * 3 && Math.Min(close[i], open[i]) - low[i] <= Math.Abs(open[i] - close[i]))
					{
						result.Add(CandlePattern.ShootingStar);
						continue;
					}

					if (close[i - 1] > open[i - 1] && open[i] > close[i] && open[i] <= close[i - 1] && open[i - 1] <= close[i] && open[i] - close[i] < close[i - 1] - open[i - 1])
					{
						result.Add(CandlePattern.BearishHarami);
						continue;
					}

					if (open[i - 1] > close[i - 1] && close[i] > open[i] && close[i] <= open[i - 1] && close[i - 1] <= open[i] && close[i] - open[i] < open[i - 1] - close[i - 1])
					{
						result.Add(CandlePattern.BullishHarami);
						continue;
					}

					if (close[i - 1] > open[i - 1] && open[i] > close[i] && open[i] >= close[i - 1] && open[i - 1] >= close[i] && open[i] - close[i] > close[i - 1] - open[i - 1])
					{
						result.Add(CandlePattern.BearishEngulfing);
						continue;
					}

					if (open[i - 1] > close[i - 1] && close[i] > open[i] && close[i] >= open[i - 1] && close[i - 1] >= open[i] && close[i] - open[i] > open[i - 1] - close[i - 1])
					{
						result.Add(CandlePattern.BullishEngulfing);
						continue;
					}

					if (open[i - 1] > close[i - 1] && open[i] >= open[i - 1] && close[i] > open[i])
					{
						result.Add(CandlePattern.BullishKicker);
						continue;
					}

					if (open[i - 1] < close[i - 1] && open[i] <= open[i - 1] && close[i] <= open[i])
					{
						result.Add(CandlePattern.BearishKicker);
						continue;
					}

					if ((close[i - 1] > open[i - 1]) && (((close[i - 1] + open[i - 1]) / 2) > close[i]) && (open[i] > close[i]) && (open[i] > close[i - 1]) && (close[i] > open[i - 1]) && ((open[i] - close[i]) / (.001m + (high[i] - low[i])) > 0.6m))
					{
						result.Add(CandlePattern.BearishDarkCloudCover);
						continue;
					}

					// These patterns require at least 3 data points
					if (i > 1)
					{
						if (close[i - 2] < open[i - 2] && Math.Max(open[i - 1], close[i - 1]) < close[i - 2] && open[i] > Math.Max(open[i - 1], close[i - 1]) && close[i] > open[i])
						{
							result.Add(CandlePattern.BullishMorningStar);
							continue;
						}

						if (close[i - 2] > open[i - 2] && Math.Min(open[i - 1], close[i - 1]) > close[i - 2] && open[i] < Math.Min(open[i - 1], close[i - 1]) && close[i] < open[i])
						{
							result.Add(CandlePattern.BearishEveningStar);
							continue;
						}

						if (((high[i] - low[i] > 4 * (open[i] - close[i])) && ((close[i] - low[i]) / (.001m + high[i] - low[i]) >= 0.75m) && ((open[i] - low[i]) / (.001m + high[i] - low[i]) >= 0.75m)) && high[i - 1] < open[i] && high[i - 2] < open[i])
						{
							result.Add(CandlePattern.BearishHangingMan);
							continue;
						}
					}

					if (i > 9)
					{
						var upper = high.Skip(i - 10).Take(10).OrderByDescending(x => x).ToList()[1];
						if (close[i - 1] < open[i - 1] && open[i] < low[i - 1] && close[i] > close[i - 1] + ((open[i - 1] - close[i - 1]) / 2) && close[i] < open[i - 1])
						{
							result.Add(CandlePattern.PiercingLine);
							continue;                     
						}

						var lower = high.Skip(i - 10).Take(10).OrderBy(x => x).ToList()[1];
						if (low[i] == open[i] && open[i] < lower && open[i] < close[i] && close[i] > ((high[i - 1] - low[i - 1]) / 2) + low[i - 1])
						{
							result.Add(CandlePattern.BullishBelt);
							continue;
						}
					}
				}

				result.Add(null);
			}

			return result;
		}
	}
}
