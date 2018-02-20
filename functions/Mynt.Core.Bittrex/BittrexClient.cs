using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Bittrex.Models;
using Newtonsoft.Json;

namespace Mynt.Core.Bittrex
{
    /// <summary>
    /// A http client for the Bittrex crypto trading platform api
    /// Api docs: https://www.bittrex.com/Home/Api
    /// </summary>
    public class BittrexClient
    {
        private readonly HttpClient _httpClient;

        private readonly string _apiKey;
        private readonly string _apiSecret;

        private readonly string _publicBaseUrl;
        private readonly string _accountBaseUrl;
        private readonly string _marketBaseUrl;
        private readonly string _publicNewBaseUrl;
        private readonly string _baseAddress;

        private readonly string _apiVersion = "v1.1";

        public BittrexClient(string apiKey, string apiSecret)
        {
            _httpClient = new HttpClient();

            _apiKey = apiKey;
            _apiSecret = apiSecret;

            _baseAddress = "https://www.bittrex.com/api";
            _publicNewBaseUrl = $"{_baseAddress}/v2.0/pub/market";
            _publicBaseUrl = $"{_baseAddress}/{_apiVersion}/public";
            _accountBaseUrl = $"{_baseAddress}/{_apiVersion}/account";
            _marketBaseUrl = $"{_baseAddress}/{_apiVersion}/market";
        }

        /// <summary>
        /// Get all available markets on bittrex
        /// Example: https://bittrex.com/api/v1.1/public/getmarkets    
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResult<List<Market>>> GetMarkets()
        {
            var url = $"{_publicBaseUrl}/getmarkets";

            var json = await GetPublicAsync(url);

            return JsonConvert.DeserializeObject<ApiResult<List<Market>>>(json);
        }

        /// <summary>
        /// Get all of the available currencies on the bittrex market
        /// Example: https://bittrex.com/api/v1.1/public/getcurrencies   
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResult<List<CryptoCurrency>>> GetCurrencies()
        {
            var url = $"{_publicBaseUrl}/getcurrencies";

            var json = await GetPublicAsync(url);

            return JsonConvert.DeserializeObject<ApiResult<List<CryptoCurrency>>>(json);
        }

        /// <summary>
        /// Get a currency ticker
        /// Example: https://bittrex.com/api/v1.1/public/getticker?market=BTC-LTC   
        /// </summary>
        /// <param name="market"></param>
        /// <returns></returns>
        public async Task<ApiResult<Ticker>> GetTicker(string market)
        {
            var url = $"{_publicBaseUrl}/getticker?market={market}";

            var json = await GetPublicAsync(url);

            return JsonConvert.DeserializeObject<ApiResult<Ticker>>(json);
        }

        /// <summary>
        /// Used to get the last 24 hour summary of all active exchanges 
        /// Example: https://bittrex.com/api/v1.1/public/getmarketsummaries   
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResult<List<MarketSummary>>> GetMarketSummaries()
        {
            var url = $"{_publicBaseUrl}/getmarketsummaries";

            var json = await GetPublicAsync(url);

            return JsonConvert.DeserializeObject<ApiResult<List<MarketSummary>>>(json);
        }
        
        /// <summary>
        /// Used to get the candles of a given market for a given period. 
        /// Example: https://bittrex.com/api/v2.0/pub/market/getticks?marketName={market}&tickInterval={period}&_={startDate}   
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResult<List<Candle>>> GetTickerHistory(string marketName, long startDate, Period period)
        {
            var url = $"{_publicNewBaseUrl}/getticks?marketName={marketName}&tickInterval={period.ToString()}&_={startDate}";

            var json = await GetPublicAsync(url);

            return JsonConvert.DeserializeObject<ApiResult<List<Candle>>>(json);
        }

        /// <summary>
        /// Used to get the last 24 hour summary of the specified market
        /// Example: https://bittrex.com/api/v1.1/public/getmarketsummary?market=btc-ltc  
        /// </summary>
        /// <param name="marketName"></param>
        /// <returns></returns>
        public async Task<ApiResult<MarketSummary>> GetMarketSummary(string marketName)
        {
            var url = $"{_publicBaseUrl}/getmarketsummary?market={marketName}";

            var json = await GetPublicAsync(url);
            
            var arrayResult = JsonConvert.DeserializeObject<ApiResult<MarketSummary[]>>(json);

            // The end point returns an array so we need to pull this out into a single MarketSummary result
            return new ApiResult<MarketSummary>(
                arrayResult.Success, 
                arrayResult.Message, 
                arrayResult.Result.FirstOrDefault());
        }

        /// <summary>
        /// Used to get retrieve the orderbook for a given market 
        /// Example: https://bittrex.com/api/v1.1/public/getorderbook?market=BTC-LTC&type=buy&depth=50    
        /// </summary>
        /// <returns></returns>
        /// TODO: Create a json converter to handle this properly with OrderType.Both
        public async Task<ApiResult<List<BookOrder>>> GetOrderBook(string market, BookOrderType orderType, int depth = 20)        {

            var typeString = orderType.ToString().ToLower();

            var url = $"{_publicBaseUrl}/getorderbook?market={market}&type={typeString}&depth={depth}";

            var json = await GetPublicAsync(url);

            var apiResult = JsonConvert.DeserializeObject<ApiResult<List<BookOrder>>>(json);

            if (!apiResult.Success)
                return apiResult;

            foreach (var order in apiResult.Result)
                order.OrderType = orderType;

            return apiResult;
        }

        /// <summary>
        /// Used to retrieve the latest trades that have occured for a specific market.
        /// Example: https://bittrex.com/api/v1.1/public/getmarkethistory?market=BTC-DOGE    
        /// </summary>
        /// <param name="marketName"></param>
        /// <returns></returns>
        public async Task<ApiResult<List<HistoricTrade>>> GetMarketHistory(string marketName)
        {
            var url = $"{_publicBaseUrl}/getmarkethistory?market={marketName}";

            var json = await GetPublicAsync(url);

            return JsonConvert.DeserializeObject<ApiResult<List<HistoricTrade>>>(json);
        }

        /// <summary>
        /// Used to place a buy order in a specific market.
        /// </summary>
        /// <param name="marketName">The name of the market to buy on, i.e. BTC-LTC.</param>
        /// <param name="quantity">The quantity of the currency to buy.</param>
        /// <param name="rate">The rate at which to buy the currency.</param>
        /// <returns></returns>
        public async Task<ApiResult<OrderResult>> BuyLimit(string marketName, double quantity, double rate)
        {
            var nonce = GenerateNonce();

            var url = $"{_marketBaseUrl}/buylimit?apiKey={_apiKey}&nonce={nonce}&market={marketName}&quantity={quantity}&rate={rate}";

            var json = await GetPrivateAsync(url, _apiSecret);

            return JsonConvert.DeserializeObject<ApiResult<OrderResult>>(json);
        }

        /// <summary>
        /// Used to place an sell order in a specific market. 
        /// </summary>
        /// <param name="marketName">The name of the market to sell on, i.e. BTC-LTC.</param>
        /// <param name="quantity">The quantity of the currency to sell.</param>
        /// <param name="rate">The rate at which to sell the currency.</param>
        /// <returns></returns>
        public async Task<ApiResult<OrderResult>> SellLimit(string marketName, double quantity, double rate)
        {
            var nonce = GenerateNonce();

            var url = $"{_marketBaseUrl}/selllimit?apiKey={_apiKey}&nonce={nonce}&market={marketName}&quantity={quantity}&rate={rate}";

            var json = await GetPrivateAsync(url, _apiSecret);

            return JsonConvert.DeserializeObject<ApiResult<OrderResult>>(json);
        }

        /// <summary>
        /// Used to cancel a buy or sell order
        /// </summary>
        /// <param name="uuid">The uniqueidentifier of the order to cancel</param>
        /// <returns></returns>
        public async Task<ApiResult<OrderResult>> CancelOrder(Guid uuid)
        {
            var nonce = GenerateNonce();

            var url = $"{_marketBaseUrl}/cancel?apiKey={_apiKey}&nonce={nonce}&uuid={uuid}";

            var json = await GetPrivateAsync(url, _apiSecret);

            return JsonConvert.DeserializeObject<ApiResult<OrderResult>>(json);
        }

        /// <summary>
        /// Get all orders that you currently have opened. A specific market can be requested.
        /// </summary>     
        public async Task<ApiResult<List<OpenOrder>>> GetOpenOrders(string marketName = null)
        {
            var nonce = GenerateNonce();

            var marketParam = BuildParameter("market", marketName);
            var url = $"{_marketBaseUrl}/getopenorders?apiKey={_apiKey}&nonce={nonce}&{marketParam}";

            var json = await GetPrivateAsync(url, _apiSecret);

            return JsonConvert.DeserializeObject<ApiResult<List<OpenOrder>>>(json);
        }

        /// <summary>
        /// Used to retrieve all balances from your account 
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResult<List<AccountBalance>>> GetBalances()
        {
            var nonce = GenerateNonce();

            var url = $"{_accountBaseUrl}/getbalances?apikey={_apiKey}&nonce={nonce}";

            var json = await GetPrivateAsync(url, _apiSecret);           

            return JsonConvert.DeserializeObject<ApiResult<List<AccountBalance>>>(json);
        }

        /// <summary>
        /// Used to retrieve the balance from your account for a specific currency. 
        /// </summary>
        /// <param name="currencyName">The ticker name of a currency, i.e. BTC</param>
        /// <returns></returns>
        public async Task<ApiResult<AccountBalance>> GetBalance(string currencyName)
        {
            var nonce = GenerateNonce();

            var url = $"{_accountBaseUrl}/getbalance?apiKey={_apiKey}&nonce={nonce}&currency={currencyName}";

            var json = await GetPrivateAsync(url, _apiSecret);

            return JsonConvert.DeserializeObject<ApiResult<AccountBalance>>(json);
        }

        /// <summary>
        /// Used to retrieve or generate an address for a specific currency. If one does not exist, the call will fail and return ADDRESS_GENERATING until one is available. 
        /// </summary>
        /// <param name="currencyName">Required: A string literal for the currency, e.g. BTC</param>
        /// <returns></returns>
        public async Task<ApiResult<DepositAddress>> GetDepositAddress(string currencyName)
        {
            var nonce = GenerateNonce();

            var url = $"{_accountBaseUrl}/getdepositaddress?apiKey={_apiKey}&nonce={nonce}&currency={currencyName}";

            var json = await GetPrivateAsync(url, _apiSecret);

            return JsonConvert.DeserializeObject<ApiResult<DepositAddress>>(json);
        }

        /// <summary>
        /// Used to retrieve your order history. 
        /// </summary>
        /// <param name="marketName">Optional string literal for a market name, i.e. BTC-LTC.</param>
        /// <returns></returns>
        public async Task<ApiResult<List<HistoricAccountOrder>>> GetOrderHistory(string marketName = null)
        {
            var marketParam = BuildParameter("market", marketName);
            var nonceParam = BuildParameter("nonce", GenerateNonce());

            var url = $"{_accountBaseUrl}/getorderhistory?apiKey={_apiKey}&{nonceParam}&{marketParam}";

            var json = await GetPrivateAsync(url, _apiSecret);

            return JsonConvert.DeserializeObject<ApiResult<List<HistoricAccountOrder>>>(json);
        }

        /// <summary>
        /// Used to retrieve a single order by uuid. 
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public async Task<ApiResult<HistoricAccountOrder>> GetOrder(Guid uuid)
        {
            var nonceParam = BuildParameter("nonce", GenerateNonce());
            var uuidParam = BuildParameter("uuid", uuid);

            var url = $"{_accountBaseUrl}/getorder?apikey={_apiKey}&{nonceParam}&{uuidParam}";

            var json = await GetPrivateAsync(url, _apiSecret);

            return JsonConvert.DeserializeObject<ApiResult<HistoricAccountOrder>>(json);
        }

        /// <summary>
        /// Execute a GET request for a public api end point
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<string> GetPublicAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Execute a GET request for a private api end point
        /// </summary>
        /// <param name="url"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        private async Task<string> GetPrivateAsync(string url, string secret)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            var apiSign = GenerateApiSign(url, secret);

            request.Headers.Add("apisign", apiSign);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private string GenerateNonce()
        {
            long nonce = (long)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);

            return nonce.ToString();
        }

        private string GenerateApiSign(string url, string secret)
        {
            var secretBytes = Encoding.ASCII.GetBytes(secret);

            using (var hmacsha512 = new HMACSHA512(secretBytes))
            {
                var hashBytes = hmacsha512.ComputeHash(Encoding.ASCII.GetBytes(url));

                return BitConverter.ToString(hashBytes).Replace("-", "");               
            }
        }

        private string BuildParameter(string parameterName, object obj)
        {
            if (obj == null)
                return string.Empty;

            return $"{parameterName}={obj}";
        }
    }
}
