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
        [Post("/api/orders?code={INSERT YOUR UNIQUE CODE FROM AZURE}")]
        Task<bool> DirectSell([Body]Trade order);
        [Get("/api/trades?code={INSERT YOUR UNIQUE CODE FROM AZURE}")]
        Task<List<Trade>> GetActiveTrades();
        [Get("/api/history?code={INSERT YOUR UNIQUE CODE FROM AZURE}")]
        Task<TradeHistory> GetHistoricTrades();
        [Get("/api/settings?code={INSERT YOUR UNIQUE CODE FROM AZURE}")]
        Task<TradeSettings> GetSettings();
        [Post("/api/notifications?code={INSERT YOUR UNIQUE CODE FROM AZURE}")]
        Task<string> Register([Body] Installation installation);
    }
}