using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Objects;
using Mynt.Core.Binance;
using Mynt.Core.Enums;
using System.Drawing;
using Mynt.Core.Models;

namespace Mynt.ResistanceSupportFinder
{
    class MainClass
    {
        private static Random rnd = new Random();

        public static void Main(string[] args)
        {
            CheckOrderBookData("NEOBTC").Wait();
        }

        private static async Task CheckOrderBookData(string market)
        {
            var n = 1;
            var xDev = (decimal)2;
            var supDev = (decimal)5;

            var api = new BinanceApi();
            var candles = await api.GetTickerHistory(market, DateTime.Now.AddDays(-500), Period.Day);
            var validSupport = new List<SupportResistanceLevel>();

            var symbolInfo = await api.GetSymbolInfo(market);
            var priceFilter = (symbolInfo.Filters.FirstOrDefault(x => x.FilterType == SymbolFilterType.PriceFilter) as BinanceSymbolPriceFilter);
            var minPrice = priceFilter.MinPrice;
            var maxPrice = priceFilter.MaxPrice;
            var tickSize = priceFilter.TickSize;

            var closes = candles.Select(x => new
            {
                Price = ClampPrice(minPrice, maxPrice, tickSize, (decimal)x.Close),
                Date = x.Timestamp
            }).ToList();

            var amount = closes.Count;

            while (closes.Count > 0)
            {
                var support = new List<Tuple<decimal, DateTime>>();
                var absoluteMinimum = closes.Select(x => x.Price).Min();
                var closeMatches = closes.Where(x => (Math.Abs((x.Price - absoluteMinimum) / absoluteMinimum) * 100) < xDev)
                    .Select(x => new Tuple<decimal, DateTime>(x.Price, x.Date)).ToList();

                // Add these to our supportLevels
                support.AddRange(closeMatches);

                // Remove these from our minimum levels.
                closes.RemoveAll(x => closeMatches.Select(y => y.Item1).Contains(x.Price));
                validSupport.Add(new SupportResistanceLevel
                {
                    Price = support.Select(x => x.Item1).Average(),
                    Hits = support.Select(x => x.Item2).ToList()
                });
            }

            var smoothedSupport = new List<SupportResistanceLevel>();
            while (validSupport.Count > 0)
            {
                var absoluteMinimum = validSupport.Select(x => x.Price).Min();
                var closeMatches = validSupport.Where(x => (Math.Abs((x.Price - absoluteMinimum) / absoluteMinimum) * 100) < supDev).Select(x => x).ToList();

                validSupport.RemoveAll(x => closeMatches.Select(y => y.Price).Contains(x.Price));

                var dates = new List<DateTime>();
                closeMatches.Select(x => x.Hits).ToList().ForEach(p => dates.AddRange(p));

                smoothedSupport.Add(new SupportResistanceLevel
                {
                    Price = closeMatches.Select(x => x.Price).Average(),
                    Hits = dates
                });
            }

            // calculate the score for amount of hits
            var result = smoothedSupport.OrderByDescending(x => x.AmountOfHits).ToList();
            for (int i = 0; i < result.Count; i++)
            {
                result[i].AmountOfHitsScore = result.Count - i;
            }

            // calculate the score for time ago it was hit
            result = result.OrderByDescending(x => x.FirstTimeHit).ToList();
            for (int i = 0; i < result.Count; i++)
            {
                result[i].FirstTimeHitScore = result.Count - i;
            }

            // calculate the score for last time it was hit
            result = result.OrderBy(x => x.FirstTimeHit).ToList();
            for (int i = 0; i < result.Count; i++)
            {
                result[i].LastTimeHitScore = result.Count - i;
            }

            var zigzags = CalculateZigZagPoints(candles);

            foreach (var item in result)
            {
                // count the amount of zigzags within 2 procent of a resistance level
                item.FractalHitsScore = zigzags.Count(x => x >= (double)(item.Price * (decimal)0.99) &&
                                                           x <= (double)(item.Price * (decimal)1.01)); ;
            }

            // Log the results to the windows as a TradingView script.
            Console.WriteLine($"study(\"Support & Resistance Levels for {market}\", overlay=true)");

            var ticker = await api.GetTicker(market);
            var minimumPrice = result.Select(x => x.Price).Min();
            var maximumPrice = result.Select(x => x.Price).Max();

            foreach (var item in result.OrderByDescending(x => x.Conviction))
            {
                Console.WriteLine(
                            $"hline(title=\"{item.Price.ToString("0.00000000", CultureInfo.InvariantCulture)}\"," +
                            $"price={item.Price.ToString("0.00000000", CultureInfo.InvariantCulture)}," +
                            $"color={(item.FractalHitsScore > 0 || item.Price == minimumPrice || item.Price == maximumPrice ? "blue" : GetColor())}, " +
                            $"linestyle={(item.Price < (decimal)ticker.Bid ? "dotted" : "solid")}, " +
                            $"linewidth={(item.FractalHitsScore > 0 || item.Price == minimumPrice || item.Price == maximumPrice ? "3" : "1")}) " +
                            $"// Hits: {item.AmountOfHitsScore} - First: {item.FirstTimeHitScore} - Last: {item.LastTimeHitScore} - Fract: {item.FractalHitsScore} - Conv: {(item.Conviction / (result.Count * 3)) * 100}%");
            }

            Console.WriteLine("plot(1)");
            Console.ReadLine();
        }

        public static List<double> CalculateZigZagPoints(List<Candle> candles)
        {
            var result = new List<double>();

            var filteredTops = new List<bool>();
            var filteredBottoms = new List<bool>();

            var topCount = new List<int>();
            var bottomCount = new List<int>();

            var highs = candles.Select(x => x.High).ToList();
            var lows = candles.Select(x => x.Low).ToList();

            for (int i = 0; i < candles.Count; i++)
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

            for (int i = 0; i < candles.Count; i++)
            {
                topCount.Add(iTop);
                bottomCount.Add(iBottom);

                iTop += 1;
                iBottom += 1;

                if (filteredTops[i]) iTop = 1;
                if (filteredBottoms[i]) iBottom = 1;
            }

            for (int i = 0; i < candles.Count; i++)
            {
                if (i > 1)
                {
                    if (filteredTops[i] && topCount[i - 1] > bottomCount[i - 1])
                        result.Add(highs[i - 2]);
                    else if (filteredBottoms[i] && topCount[i - 1] < bottomCount[i - 1])
                        result.Add(lows[i - 2]);
                }
            }

            return result;
        }

        public static string GetColor()
        {
            var randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            return ColorTranslator.ToHtml(randomColor);
        }

        public static decimal ClampPrice(decimal minPrice, decimal maxPrice, decimal tickSize, decimal price)
        {
            price = Math.Min(maxPrice, price);
            price = Math.Max(minPrice, price);
            price -= price % tickSize;
            price = Floor(price);
            return price;
        }

        private static decimal Floor(decimal number)
        {
            return Math.Floor(number * 100000000) / 100000000;
        }
    }
}



