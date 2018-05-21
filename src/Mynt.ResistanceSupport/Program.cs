using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Models;
using Mynt.ResistanceSupport.Models;
using Mynt.Core.Extensions;

namespace Mynt.ResistanceSupport
{
	class Program
	{
		private static readonly Random rnd = new Random();

		private static List<PivotPoint> _pivotLevels = new List<PivotPoint>();
		private static List<StandardDevLevel> _stdDevLevels = new List<StandardDevLevel>();
		private static List<decimal> _zigZagLevels = new List<decimal>();
		private static List<decimal> _fibLevels = new List<decimal>();
		private static List<decimal> _finalLevels = new List<decimal>();
		private static List<Candle> _candles = new List<Candle>();
		private static BaseExchange _api;

		private static string _market = "ETCBTC";

		static void Main(string[] args)
		{
			try
			{
				// Init an API
				_api = new BaseExchange(new ExchangeOptions() { Exchange = Exchange.Binance, ApiKey = "Nope", ApiSecret = "Nah..." });

				Console.WriteLine("Give me a coin!");

				var coin = Console.ReadLine();

				if (!string.IsNullOrEmpty(coin.Trim()))
					_market = $"{coin}BTC";

				// Retrieve our candle data to create the SR levels...
				Console.WriteLine("Gathering candle data...");
				GetCandleData().Wait();
				Console.WriteLine($"Candle data received {_candles.Count}...");

				Console.WriteLine("Checking levels based on pivot points...");
				CalculatePivotPoints();
				Console.WriteLine($"Found {_pivotLevels.Count} pivot points...");

				Console.WriteLine("Checking levels based on fibs...");
				CalculateFibs();
				Console.WriteLine($"Found {_fibLevels.Count} fibs...");

				Console.WriteLine("Checking levels based on zig zag fractals...");
				CalculateZigZagPoints();
				Console.WriteLine($"Found {_zigZagLevels.Count} zig zag levels...");

                Console.WriteLine("Checking levels based on standard dev...");
                CalculateStandardDev();
                Console.WriteLine($"Found {_stdDevLevels.Count} standard dev levels...");

				Console.WriteLine("Consolidating levels...");
				ConsolidateLevels();
				Console.WriteLine($"Ended up with {_finalLevels.Count} levels...");

				Console.WriteLine();

				WriteToTradingViewScript();

				Console.ReadLine();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.ReadLine();
			}
		}

		private static async Task GetCandleData()
		{
			// Get the max amount of candles to check.
			_candles = await _api.GetTickerHistory(_market, Period.Day, 1000);
		}

		private static void ConsolidateLevels()
		{
			var prices = new List<decimal>();

            // Fibs are always interesting levels.
			_finalLevels.AddRange(_fibLevels);

			// TODO: Figure out a way to determine which other levels are most interesting...		
		}

		#region fibs

		public static void CalculateFibs()
		{
			var highPoint = _pivotLevels.OrderByDescending(x => x.Price).FirstOrDefault();
			var lowPoint = _pivotLevels.OrderBy(x => x.Price).FirstOrDefault(x => x.Timestamp > highPoint.Timestamp);

			_fibLevels.Add(lowPoint.Price);
			_fibLevels.Add(Math.Round(lowPoint.Price + ((highPoint.Price - lowPoint.Price) * .236m), 8));
			_fibLevels.Add(Math.Round(lowPoint.Price + ((highPoint.Price - lowPoint.Price) * .382m), 8));
			_fibLevels.Add(Math.Round(lowPoint.Price + ((highPoint.Price - lowPoint.Price) * .5m), 8));
			_fibLevels.Add(Math.Round(lowPoint.Price + ((highPoint.Price - lowPoint.Price) * .618m), 8));
			_fibLevels.Add(Math.Round(lowPoint.Price + ((highPoint.Price - lowPoint.Price) * .786m), 8));
			_fibLevels.Add(highPoint.Price);
		}

		#endregion

		#region standard dev

		private static void CalculateStandardDev()
		{
			var dev = 2.0m;         
			var validSupport = new List<StandardDevLevel>();
			var candles = new List<Candle>(_candles); // Create a clone...

			// Get all the closes within the standard deviation
			while (candles.Count > 0)
			{
				var stdDevLevel = new StandardDevLevel();
				var absoluteMinimum = candles.Close().Min();
				var closeMatches = candles.Where(x => (Math.Abs((x.Close - absoluteMinimum) / absoluteMinimum) * 100) < dev).OrderBy(y => y.Timestamp).ToList();

				stdDevLevel.Price = absoluteMinimum;
				stdDevLevel.Matches = closeMatches;

				// Remove these from our minimum levels.
				candles.RemoveAll(x => closeMatches.Contains(x));

				validSupport.Add(stdDevLevel);
			}

			_stdDevLevels = validSupport;

			// Smooth the result out
			var smoothedSupport = new List<StandardDevLevel>();
			var smoothDev = 5.0m;

			while (validSupport.Count > 0)
			{
				var stdDevLevel = new StandardDevLevel();
				var absoluteMinimum = validSupport.Select(x => x.Price).Min();
				var closeMatches = validSupport.Where(x => (Math.Abs((x.Price - absoluteMinimum) / absoluteMinimum) * 100) < smoothDev).ToList();

                stdDevLevel.Price = absoluteMinimum;

                foreach(var item in closeMatches)
					stdDevLevel.Matches.AddRange(item.Matches);

				stdDevLevel.Matches = stdDevLevel.Matches.OrderBy(x => x.Timestamp).ToList();

				validSupport.RemoveAll(x => closeMatches.Contains(x));
				smoothedSupport.Add(stdDevLevel);
			}

			_stdDevLevels = smoothedSupport;

			var maxPeriod = _stdDevLevels.Select(x => x.HitPeriod).Max();
			var maxCount = _stdDevLevels.Select(x => x.Matches.Count).Max();
   
            // Remove levels with not enough interaction...
			_stdDevLevels.RemoveAll(y => y.HitPeriod < maxPeriod * 0.5);
            _stdDevLevels.RemoveAll(y => y.Matches.Count < maxCount * 0.5);
		}

		#endregion

		#region zig zag fractals

		public static void CalculateZigZagPoints()
		{
			var result = new List<decimal>();
			var filteredTops = new List<bool>();
			var filteredBottoms = new List<bool>();

			var topCount = new List<int>();
			var bottomCount = new List<int>();

			var highs = _candles.Select(x => x.High).ToList();
			var lows = _candles.Select(x => x.Low).ToList();

			for (int i = 0; i < _candles.Count; i++)
			{
				if (i < 5)
				{
					filteredTops.Add(false);
					filteredBottoms.Add(false);
				}
				else
				{
					if (highs[i - 4] < highs[i - 2] && highs[i - 3] <= highs[i - 2] && highs[i - 2] >= highs[i - 1] &&
						highs[i - 2] > highs[i])
						filteredTops.Add(true);
					else
						filteredTops.Add(false);

					if (lows[i - 4] > lows[i - 2] && lows[i - 3] >= lows[i - 2] && lows[i - 2] <= lows[i - 1] &&
						lows[i - 2] < lows[i])
						filteredBottoms.Add(true);
					else
						filteredBottoms.Add(false);
				}
			}

			var iTop = 1;
			var iBottom = 1;

			for (int i = 0; i < _candles.Count; i++)
			{
				topCount.Add(iTop);
				bottomCount.Add(iBottom);

				iTop += 1;
				iBottom += 1;

				if (filteredTops[i]) iTop = 1;
				if (filteredBottoms[i]) iBottom = 1;
			}

			for (int i = 0; i < _candles.Count; i++)
			{
				if (i > 1)
				{
					if (filteredTops[i] && topCount[i - 1] > bottomCount[i - 1])
						result.Add(highs[i - 2]);
					else if (filteredBottoms[i] && topCount[i - 1] < bottomCount[i - 1])
						result.Add(lows[i - 2]);
				}
			}

			_zigZagLevels = result.Distinct().ToList();
		}

		#endregion

		#region pivot points

		private static void CalculatePivotPoints()
		{
			// Get all the swing highs and swing lows
			var multiplier = 1;
			var upperPivots = MultiplyPivots(multiplier, _candles.Select(x => new PivotPoint { Price = x.Close, Timestamp = x.Timestamp, Type = PivotType.Upper }).ToList(), true);
			var lowerPivots = MultiplyPivots(multiplier, _candles.Select(x => new PivotPoint { Price = x.Close, Timestamp = x.Timestamp, Type = PivotType.Lower }).ToList(), false);

			_pivotLevels.AddRange(lowerPivots);
			_pivotLevels.AddRange(upperPivots);

            // The high and low is interesting in any way
			var highPoint = _pivotLevels.OrderByDescending(x => x.Price).FirstOrDefault().Price;
			var lowPoint =  _pivotLevels.OrderBy(x => x.Price).FirstOrDefault().Price;
			_finalLevels.Add(highPoint);
			_finalLevels.Add(lowPoint);
		}

		private static List<PivotPoint> MultiplyPivots(double multiplier, List<PivotPoint> pivots, bool isUpper)
		{
			var newPivots = new List<PivotPoint>();

			for (int i = 1; i < pivots.Count - 2; i++)
			{
				if (!isUpper && pivots[i].Price < pivots[i - 1].Price && pivots[i].Price < pivots[i + 1].Price)
				{
					// This is a swing low
					newPivots.Add(pivots[i]);
				}
				else if (isUpper && pivots[i].Price > pivots[i - 1].Price && pivots[i].Price > pivots[i + 1].Price)
				{
					// This is a swing high
					newPivots.Add(pivots[i]);
				}
			}

			if (multiplier > 0)
				return MultiplyPivots(multiplier - 1, newPivots, isUpper);

			return newPivots;
		}

		#endregion

		#region helpers

		private static void WriteToTradingViewScript()
		{
			Console.WriteLine($"study(\"Support & Resistance Levels for {_market}\", overlay=true)");

			WritePivotScript();
			WriteFibScript();
			WriteStdDevScript();
			WriteZigZagScript();

			WriteFinalLevels();

			Console.ReadLine();
		}

		private static void WriteFinalLevels()
		{
            Console.WriteLine();
            Console.WriteLine("// FINAL LEVELS");
            Console.WriteLine();

            var color = GetColor();

			foreach (var item in _finalLevels)
				Console.WriteLine($"hline(title=\"{item.ToString("0.00000000", CultureInfo.InvariantCulture)}\"," +
								  $"price={item.ToString("0.00000000", CultureInfo.InvariantCulture)},color={color}, linestyle=solid, linewidth=3)");
		}

		private static void WriteZigZagScript()
		{
            Console.WriteLine();
            Console.WriteLine("// Levels based on ZigZag");
            Console.WriteLine();

            var color = GetColor();

            foreach (var item in _zigZagLevels)
                Console.WriteLine($"hline(title=\"{item.ToString("0.00000000", CultureInfo.InvariantCulture)}\"," +
                                  $"price={item.ToString("0.00000000", CultureInfo.InvariantCulture)},color={color}, linestyle=solid, linewidth=3)");
		}

		private static void WriteStdDevScript()
		{
			Console.WriteLine();
			Console.WriteLine("// Levels based on standard deviation");
			Console.WriteLine();

			var color = GetColor();

			foreach (var item in _stdDevLevels)
				Console.WriteLine($"hline(title=\"{item.Price.ToString("0.00000000", CultureInfo.InvariantCulture)}\"," +
								  $"price={item.Price.ToString("0.00000000", CultureInfo.InvariantCulture)},color={color}, linestyle=solid, linewidth=3)" +
								  $" // Period {item.HitPeriod} | Count {item.Matches.Count} | First {item.FirstTimeHit} | Last {item.LastTimeHit}");
		}

		private static void WriteFibScript()
		{
			Console.WriteLine();
			Console.WriteLine("// Fib levels");
			Console.WriteLine();

			var color = GetColor();

			foreach (var item in _fibLevels)
				Console.WriteLine($"hline(title=\"{item.ToString("0.00000000", CultureInfo.InvariantCulture)}\"," +
								  $"price={item.ToString("0.00000000", CultureInfo.InvariantCulture)},color={color}, linestyle=solid, linewidth=3)");
		}

		private static void WritePivotScript()
		{
			var first = true;

			Console.WriteLine();
			Console.WriteLine("// Pivot points");
			Console.WriteLine();
			Console.Write("upperPivots = ");

			foreach (var item in _pivotLevels.Where(x => x.Type == PivotType.Upper))
			{
				if (!first) Console.Write(" or ");
				Console.Write($"time == timestamp({item.Timestamp.Year}, {item.Timestamp.Month}, {item.Timestamp.Day}, 0, 0)");
				first = false;
			}

			Console.WriteLine();
			Console.Write("lowerPivots = ");
			first = true;

			foreach (var item in _pivotLevels.Where(x => x.Type == PivotType.Lower))
			{
				if (!first) Console.Write(" or ");
				Console.Write($"time == timestamp({item.Timestamp.Year}, {item.Timestamp.Month}, {item.Timestamp.Day}, 0, 0)");
				first = false;
			}

			Console.WriteLine();
			Console.WriteLine("plotshape(upperPivots, style = shape.triangledown, location = location.abovebar, color = green, size = size.tiny)");
			Console.WriteLine("plotshape(lowerPivots, style = shape.triangleup, location = location.belowbar, color = red, size = size.tiny)");
		}

		public static string GetColor()
		{
			var randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
			var color = randomColor.Name.Substring(2, randomColor.Name.Length - 2);
			return $"#{color}";
		}

		#endregion
	}
}
