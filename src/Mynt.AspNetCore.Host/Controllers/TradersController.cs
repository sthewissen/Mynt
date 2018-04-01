using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;
using Mynt.Core.TradeManagers;

namespace Mynt.AspNetCore.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class TradersController : Controller
    {
        private IDataStore _manager;
        public TradersController(IDataStore manager)
        {
            _manager = manager;
        }
        // GET api/values
        [HttpGet]
        public async Task<List<Trader>> Get()
        {
            return await _manager.GetTradersAsync();
        }
    }
}
