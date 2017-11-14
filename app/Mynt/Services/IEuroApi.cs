using System;
using System.Threading.Tasks;
using Mynt.Models;
using Refit;

namespace Mynt.Services
{
    [Headers("Accept: application/json")]
    public interface IEuroApi
    {
        [Get("/nl/ticker")]
        Task<Prices> GetEuroPrice();
    }
}
