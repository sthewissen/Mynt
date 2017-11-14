using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Models;
using Refit;

namespace Mynt.Services
{
    [Headers("Accept: application/json")]
    public interface IBittrexApi
    {
        [Get("/api/v2.0/pub/market/getticks?marketName={market}&tickInterval=fiveMin")]
        Task<ApiResult<List<Candle>>> GetCandles(string market);
    }
}
