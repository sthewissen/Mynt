using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

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
}