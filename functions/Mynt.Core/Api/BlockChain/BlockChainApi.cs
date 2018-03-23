using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Refit;

namespace Mynt.Core.Api.BlockChain
{
    public static class BlockChainApi
    {
        private static IBlockChainApi _api;

        public static IBlockChainApi Instance
        {
            get
            {
                EnsureApi();

                return _api;
            }
        }
        private static void EnsureApi()
        {
            if (_api != null) return;

            var settings = new Constants();
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(settings.BlockChainApiRoot)
            };

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Init our API project
            _api = RestService.For<IBlockChainApi>(httpClient);
        }
    }
}
