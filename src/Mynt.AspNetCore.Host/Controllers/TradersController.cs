using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Mynt.Core.Backtester;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;
using Newtonsoft.Json.Linq;

namespace Mynt.AspNetCore.Host.Controllers
{
    [Route("api/[controller]")]
    public class TradersController : Controller
    {
        private readonly IDataStore _dataStore;

        public TradersController(IDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        [HttpGet]
        public async Task<List<Trader>> Get()
        {
            return await _dataStore.GetTradersAsync();
        }
    }

    [Route("api/backtest/getcacheage")]
    public class BacktestGetCacheAge : Controller
    {
        [HttpGet]
        public async Task<JArray> Get()
        {
            //Sample init with custom Values:
            string exchange = "Binance";
            BacktestOptions backtestOptions = new BacktestOptions();
            backtestOptions.Exchange = (Exchange)Enum.Parse(typeof(Exchange), exchange);
            backtestOptions.Coins = new List<string>(new string[] { "NEOBTC", "OMGBTC", "ARKBTC", "XRPBTC", "REQBTC", "LTCBTC", "ETHBTC", "VENBTC" });

            return await DataRefresher.GetCacheAge(backtestOptions, Startup.BacktestDataStore);
        }
    }

    [Route("api/backtest/refresh")]
    public class BacktestRefresh : Controller
    {
        [HttpGet]
        public async Task<string> Get()
        {
            //Sample init with custom Values:
            string exchange = "Binance";
            BacktestOptions backtestOptions = new BacktestOptions();
            backtestOptions.Exchange = (Exchange)Enum.Parse(typeof(Exchange), exchange);
            backtestOptions.Coins = new List<string>(new string[] { "NEOBTC", "OMGBTC", "ARKBTC", "XRPBTC", "REQBTC", "LTCBTC", "ETHBTC", "VENBTC" });

            await DataRefresher.RefreshCandleData((x) => Console.WriteLine(x), backtestOptions, Startup.BacktestDataStore);

            return "Refresh Done";
        }
    }

    [Route("api/backtest/all")]
    public class BacktestAllController : Controller
    {
        [HttpGet]
        public async Task<JArray> Get()
        {
            //Sample init with custom Values:
            string exchange = "Binance";
            BacktestOptions backtestOptions = new BacktestOptions();
            backtestOptions.Exchange = (Exchange)Enum.Parse(typeof(Exchange), exchange);
            backtestOptions.Coins =  new List<string>(new string[] { "NEOBTC", "OMGBTC", "ARKBTC", "XRPBTC", "REQBTC", "LTCBTC", "ETHBTC", "VENBTC" });

            return await BacktestFunctions.BackTestAllJson(backtestOptions, Startup.BacktestDataStore);
        }
    }
}