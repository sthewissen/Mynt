using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Mynt.Models;
using Refit;

namespace Mynt.Services
{
    public class BittrexApi
    {
        private HttpClient _httpClient;

        public async Task<List<Candle>> GetCandles(string market)
        {
            var api = RestService.For<IBittrexApi>(CreateClient());
            var result = await api.GetCandles(market);

            if (result.Success)
            {
                return result.Result;
            }
            else
            {
                return new List<Candle>();
            }
        }


        private HttpClient CreateClient()
        {
            if (_httpClient == null)
            {
                var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(Constants.BittrexApiRoot)
                };

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient = httpClient;
            }

            return _httpClient;
        }
    }
}
