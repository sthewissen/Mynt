using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Objects;
using Mynt.Core.Binance;
using Mynt.Core.Enums;
using System.Drawing;

namespace Mynt.ResistanceSupportFinder
{
    class MainClass
    {
        private static Random rnd = new Random();

        public static void Main(string[] args)
        {
            CheckOrderBookData("ETHBTC").Wait();
        }

        private static async Task CheckOrderBookData(string market)
        {
            var n = 1;
            var xDev = (decimal) 2;
            var supDev = (decimal)5;

            var api = new BinanceApi();
            var candles = await api.GetTickerHistory(market, DateTime.Now.AddDays(-500), Period.Day);
            var validSupport = new List<Tuple<decimal, int, int>>();

            var symbolInfo = await api.GetSymbolInfo(market);
            var priceFilter = (symbolInfo.Filters.FirstOrDefault(x => x.FilterType == SymbolFilterType.PriceFilter) as BinanceSymbolPriceFilter);
            var minPrice = priceFilter.MinPrice;
            var maxPrice = priceFilter.MaxPrice;
            var tickSize = priceFilter.TickSize;
            
            var closes = candles.Select(x=>new {Price = ClampPrice(minPrice, maxPrice, tickSize, (decimal) x.Close), Date = x.Timestamp}).ToList();
            var amount = closes.Count;

            while (closes.Count > 0)
            {
                var support = new List<Tuple<decimal, int>>();
                var absoluteMinimum = closes.Select(x=>x.Price).Min();
                var closeMatches = closes.Where(x => (Math.Abs((x.Price - absoluteMinimum) / absoluteMinimum) * 100) < xDev).Select(x => new Tuple<decimal, int>(x.Price, (DateTime.UtcNow- x.Date).Days)).ToList();

                // Add these to our supportLevels
                support.AddRange(closeMatches);

                // Remove these from our minimum levels.
                closes.RemoveAll(x => closeMatches.Select(y=>y.Item1).Contains(x.Price));
                validSupport.Add(new Tuple<decimal, int, int>(support.Select(x=>x.Item1).Average(), support.Count, support.Select(x=>x.Item2).Sum()));
            }

            var smoothedSupport = new List<Tuple<decimal, int, int>>();
            while (validSupport.Count > 0)
            {
                var absoluteMinimum = validSupport.Select(x=>x.Item1).Min();
                var closeMatches = validSupport.Where(x => (Math.Abs((x.Item1 - absoluteMinimum) / absoluteMinimum) * 100) < supDev).Select(x => x).ToList();

                validSupport.RemoveAll(x => closeMatches.Select(y=>y.Item1).Contains(x.Item1));

                smoothedSupport.Add(new Tuple<decimal, int, int>(closeMatches.Select(x=>x.Item1).Average(), closeMatches.Select(x => x.Item2).Sum(), closeMatches.Select(x => x.Item3).Sum()));
            }
            
            var result = smoothedSupport.OrderByDescending(x => x.Item2*x.Item3).ToList();

            Console.WriteLine($"study(\"Support & Resistance Levels for {market}\", overlay=true)");

            foreach (var item in result)
            {
                var isLeading = item.Item2 > amount / result.Count;
                
                Console.WriteLine(
                    $"hline(title=\"{item.Item1.ToString("0.00000000", CultureInfo.InvariantCulture)}\"," +
                    $"price={item.Item1.ToString("0.00000000", CultureInfo.InvariantCulture)}," +
                    $"color={GetColor()}, " +
                    $"linestyle={(isLeading ? "dotted" : "solid")}, " +
                    $"linewidth={(isLeading ? "3" : "1")}) // Weight: {item.Item2} - Day spread: {item.Item3}");
            }

            Console.WriteLine("plot(1)");
            Console.ReadLine();
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

