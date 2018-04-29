using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mynt.Core.Interfaces;

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
            ViewBag.traders = _dataStore.GetTradersAsync().Result;

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
