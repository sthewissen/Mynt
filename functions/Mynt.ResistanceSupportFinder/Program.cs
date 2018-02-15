using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Core.Binance;

namespace Mynt.ResistanceSupportFinder
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            CheckOrderBookData().Wait();
        }

        private static async Task CheckOrderBookData()
        {
            var api = new BinanceApi();
            var orderbook = await api.GetOrderBook("ETHBTC");

            var groupedMinimums = new List<Tuple<decimal, decimal>>();
        }
    }
}
