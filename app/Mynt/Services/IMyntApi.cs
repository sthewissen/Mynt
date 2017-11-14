using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Helpers;
using Mynt.Models;
using Refit;

namespace Mynt.Services
{
    [Headers("Accept: application/json")]
    public interface IMyntApi
    {
        [Post("/api/orders?code=jzUauHP8lEUPZvSCW1Pi9W1mEeJLNky94g6E8/LqZzvqfGOxaHHV/A==")]
        Task<bool> DirectSell([Body]Trade order);
        [Get("/api/trades?code=i/iS2/5iBfg1kLPMQrXWyIr8xfL5OUBNbgHe/hxDjIfRI8bf63D9eg==")]
        Task<List<Trade>> GetActiveTrades();
        [Get("/api/history?code=ckw82JgVpyf28xQ3u6Z2xVcwKATPqohK5aZQhWWQQocpcX98huLO2Q==")]
        Task<TradeHistory> GetHistoricTrades();
        [Get("/api/settings?code=s59/gME3ANefLDB0tXXDXO8pMiQZDagIjUVxVZZOTSWVfL35PbvDcA==")]
        Task<TradeSettings> GetSettings();
        [Post("/api/notifications?code=taStKFgVxy2vKBqLwOyoCKPCRu5ZgJvR7UaBzk656vywtzZpLGjFCg==")]
        Task<string> Register([Body] Installation installation);
    }
}