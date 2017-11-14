using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Mynt.Models;
using Refit;
using System.Linq;
using Mynt.Helpers;

namespace Mynt.Services
{
    public class MyntApi
    {
        private HttpClient _httpClient;

        public Task<List<Trade>> GetActiveTrades()
        {
            if (string.IsNullOrEmpty(Settings.FunctionRoot))
                return Task.FromResult(new List<Trade>());

            var api = RestService.For<IMyntApi>(CreateClient());
            return api.GetActiveTrades();
        }

        public Task<bool> DirectSell(Trade order)
        {
            if (string.IsNullOrEmpty(Settings.FunctionRoot))
                return Task.FromResult(false);

            var api = RestService.For<IMyntApi>(CreateClient());
            return api.DirectSell(order);
        }

        public Task<TradeHistory> GetHistoricTrades()
        {
            if (string.IsNullOrEmpty(Settings.FunctionRoot))
                return Task.FromResult(new TradeHistory());

            var api = RestService.For<IMyntApi>(CreateClient());
            return api.GetHistoricTrades();
        }

        public Task<TradeSettings> GetSettings()
        {
            if (string.IsNullOrEmpty(Settings.FunctionRoot))
                return Task.FromResult(new TradeSettings());

            var api = RestService.For<IMyntApi>(CreateClient());
            return api.GetSettings();
        }

        public Task<string> Register(Installation installation)
        {
            if (string.IsNullOrEmpty(Settings.FunctionRoot))
                return Task.FromResult(string.Empty);

            var api = RestService.For<IMyntApi>(CreateClient());
            return api.Register(installation);
        }

        HttpClient CreateClient()
        {
            if (_httpClient == null && !string.IsNullOrEmpty(Settings.FunctionRoot))
            {
                _httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(Constants.ApiBaseUrl)
                };

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            return _httpClient;
        }
    }
}
