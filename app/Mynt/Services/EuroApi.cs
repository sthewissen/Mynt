using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Mynt.Models;
using Refit;

namespace Mynt.Services
{
    public class EuroApi
    {
        private HttpClient _httpClient;

        public Task<Prices> GetEuroPrice()
        {
            var api = RestService.For<IEuroApi>(CreateClient());
            return api.GetEuroPrice();
        }


        private HttpClient CreateClient()
        {
            if (_httpClient == null)
            {
                var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(Constants.BlockChainApiRoot)
                };

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient = httpClient;
            }

            return _httpClient;
        }
    }
}
