using System;
using System.Collections.Generic;
using System.Configuration;
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
        private readonly IDataStore _dataStore;

        public MyntController(IDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        // GET: /<controller>/
        public IActionResult Dashboard()
        {
            var tradeOptions = Startup.Configuration.GetSection("TradeOptions").Get<TradeOptions>();

            ViewBag.quoteCurrency = tradeOptions.QuoteCurrency;
            ViewBag.traders = _dataStore.GetTradersAsync().Result;
            ViewBag.closedTrades = _dataStore.GetClosedTradesAsync().Result;

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
