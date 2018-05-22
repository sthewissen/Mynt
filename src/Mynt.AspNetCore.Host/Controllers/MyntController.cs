using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Mynt.Core.Interfaces;
using Mynt.Core.TradeManagers;

namespace Mynt.AspNetCore.Host.Controllers
{
    public class MyntController : Controller
    {
        private readonly IExchangeApi _api;
        private readonly IDataStore _dataStore;

        public MyntController(IDataStore dataStore, IExchangeApi api)
        {
            _dataStore = dataStore;
            _api = api;
        }

        // GET: /<controller>/        
        public async Task<IActionResult> Dashboard()
        {
            var tradeOptions = Startup.Configuration.GetSection("TradeOptions").Get<TradeOptions>();

            ViewBag.quoteCurrency = tradeOptions.QuoteCurrency;
            // Get active trades
            var activeTrades = await _dataStore.GetActiveTradesAsync();
            ViewBag.activeTrades = activeTrades;

            // Get current prices
            //ExchangeSharp.

            // Get Traders
            var traders = await _dataStore.GetTradersAsync();
            ViewBag.traders = traders;

            // Check if Trader has active trade
            foreach (var trader in traders)
            {
                if (activeTrades.Count > 0)
                {
                    var activeTrade = activeTrades.Where(t => t.TraderId == trader.Identifier).ToList();
                    if (activeTrade.Count >= 1)
                    {
                        trader.ActiveTrade = activeTrade.First();

                        //Temp for shortened
                        var actT = trader.ActiveTrade;

                        // Get Tickers
                        var ticker = await _api.GetTicker(actT.Market);
                        trader.ActiveTrade.OpenProfit = actT.OpenRate - ticker.Last;
                        trader.ActiveTrade.OpenProfitPercentage = ((actT.OpenRate + actT.OpenProfit) - actT.OpenRate) * 100;
                    }
                }

                // Check Profit/Loss
                trader.ProfitLoss = ((100 * trader.CurrentBalance) / trader.StakeAmount) - 100;
            }

            ViewBag.closedTrades = await _dataStore.GetClosedTradesAsync();

            return View();
        }


        // GET: /<controller>/
        public IActionResult Log()
        {
            // Get log from today
            string date = DateTime.Now.ToString("yyyyMMdd");
            ViewBag.log = Controllers.Log.ReadTail("Logs/Mynt-" + date + ".log", 100);

            return View();
        }
    }
}
