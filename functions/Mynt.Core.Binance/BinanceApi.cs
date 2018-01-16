using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinanceExchange.API.Client;
using BinanceExchange.API.Models.Request;
using BinanceExchange.API.Models.Response;
using Mynt.Core.Api;
using Mynt.Core.Binance.Models;
using Mynt.Core.Models;

namespace Mynt.Core.Binance
{
    public class BinanceApi : IExchangeApi
    {
        private readonly IBinanceClient _client;

        public BinanceApi(string apiKey, string secretKey)
        {
            // Initialise the general client with config
            _client = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = apiKey,
                SecretKey = secretKey,
            });
        }
        
        public async Task<string> Buy(string market, double quantity, double rate)
        {
            var request = new CreateOrderRequest
            {
                Symbol = market,
                Quantity = (decimal)quantity,
                Price = rate,
                Type = BinanceExchange.API.Enums.OrderType.Limit,
                Side = BinanceExchange.API.Enums.OrderSide.Buy
            };

            var result = await _client.CreateOrder(request);

            return result.ClientOrderId;
        }

        public async Task<string> Sell(string market, double quantity, double rate)
        {
            var request = new CreateOrderRequest
            {
                Symbol = market,
                Quantity = (decimal)quantity,
                Price = rate,
                Type = BinanceExchange.API.Enums.OrderType.Limit,
                Side = BinanceExchange.API.Enums.OrderSide.Sell
            };

            var result = await _client.CreateOrder(request);

            return result.ClientOrderId;
        }

        public async Task<double> GetBalance(string currency)
        {
            var result = await _client.GetAccountInformation();
            var currencyInformation = result.Balances.SingleOrDefault(_ => _.Asset == currency);
            return (double)(currencyInformation.Free + currencyInformation.Locked);
        }

        public async Task<List<MarketSummary>> GetMarketSummaries()
        {
            var symbols = await _client.GetSymbolsPriceTicker();
            var tasks = symbols.Select(async _=> Tuple.Create<string,SymbolPriceChangeTickerResponse>(_.Symbol, await _client.GetDailyTicker(_.Symbol)));
            var tickers = await Task.WhenAll(tasks);
            return tickers.Select(_ =>
                new MarketSummary
                {
                    Ask= (double)_.Item2.AskPrice,
                    Bid= (double)_.Item2.BidPrice,
                    High= (double)_.Item2.HighPrice,
                    Last=(double)_.Item2.LastPrice,
                    //Low=(double)_.Item2.LowPrice,
                    MarketName = _.Item1,
                    PrevDay = (double)_.Item2.OpenPrice,
                    Volume = (double)_.Item2.Volume, 
                }).ToList();
        }

        public async Task<List<OpenOrder>> GetOpenOrders(string market)
        {
            throw new NotImplementedException();

            var request = new CurrentOpenOrdersRequest { Symbol = market };
            var result = await _client.GetCurrentOpenOrders(request);

            return result.Select(_ =>
                new OpenOrder
                {
                    CancelInitiated= (_.Status ==BinanceExchange.API.Enums.OrderStatus.Cancelled || _.Status == BinanceExchange.API.Enums.OrderStatus.PendingCancel),
                    
                }).ToList();
        }

        public async Task<Ticker> GetTicker(string market)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Candle>> GetTickerHistory(string market, long startDate, Period period)
        {
            throw new NotImplementedException();
        }
    }
}