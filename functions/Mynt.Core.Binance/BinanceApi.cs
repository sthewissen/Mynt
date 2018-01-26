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
            var tasks = symbols.Select(async _ => Tuple.Create<string, SymbolPriceChangeTickerResponse, KlineCandleStickResponse>(
                _.Symbol, await _client.GetDailyTicker(_.Symbol), await GetLastDaysCandle(_.Symbol)));
            var tickers = await Task.WhenAll(tasks);
            return tickers.Select(_ =>
                new MarketSummary
                {
                    Ask= (double)_.Item2.AskPrice,
                    BaseVolume = (double)_.Item3.TakerBuyQuoteAssetVolume,
                    Bid = (double)_.Item2.BidPrice,
                    High = (double)_.Item3.High,
                    Last=(double)_.Item3.Close,
                    Low = (double)_.Item3.Low,
                    MarketName = _.Item1,
                    PrevDay = (double)_.Item3.Open,
                    Volume = (double)_.Item2.Volume,
                }).ToList();
        }

        public async Task<List<OpenOrder>> GetOpenOrders(string market)
        {
            var request = new CurrentOpenOrdersRequest { Symbol = market  };
            var result = await _client.GetCurrentOpenOrders(request);

            return result.Select(_ =>
                new OpenOrder
                {
                    CancelInitiated = (_.Status == BinanceExchange.API.Enums.OrderStatus.Cancelled || _.Status == BinanceExchange.API.Enums.OrderStatus.PendingCancel),
                    Exchange = _.Symbol,
                    ImmediateOrCancel = (_.TimeInForce == BinanceExchange.API.Enums.TimeInForce.IOC),
                    Limit = (double)_.StopPrice,
                    OrderType = $"{_.Type.ToString()}_{_.Side.ToString()}".ToUpper(),
                    OrderUuid = _.OrderId.ToString(),
                    Price = (double)(_.Price * _.OriginalQuantity),
                    PricePerUnit = (double)_.Price,
                    Quantity = (double)_.OriginalQuantity,
                    QuantityRemaining = (double)(_.OriginalQuantity - _.ExecutedQuantity)
                }).ToList();
        }

        public async Task<Ticker> GetTicker(string market)
        {
            var result = await _client.GetDailyTicker(market);

            return new Ticker
            {
                Ask = (double)result.AskPrice,
                Bid = (double)result.BidPrice,
                Last = (double)result.LastPrice
            };
        }

        public async Task<List<Candle>> GetTickerHistory(string market, DateTime startDate, Period period)
        {
            var endTime = DateTime.UtcNow;
            var start = startDate;
            var candles =new List<KlineCandleStickResponse>();
            while (start < endTime)
            {
                var request = new GetKlinesCandlesticksRequest
                {
                    Symbol = market,
                    Interval = (BinanceExchange.API.Enums.KlineInterval)period,
                    StartTime = start,
                    EndTime = endTime
                };

                var candlesticksToAdd = await _client.GetKlinesCandlesticks(request);
                candles.AddRange(candlesticksToAdd);
                start = candlesticksToAdd.Count() == 0 ? DateTime.MaxValue : candlesticksToAdd.Max(_ => _.CloseTime);
            }

            return candles.Select(_ =>
                new Candle
                {
                    Close = (double)_.Close,
                    High=(double)_.High,
                    Low=(double)_.Low,
                    Open=(double)_.Open,
                    Timestamp=_.OpenTime
                }
            ).ToList();
        }

        private async Task<KlineCandleStickResponse> GetLastDaysCandle(string symbol)
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddDays(-1);
            var request = new GetKlinesCandlesticksRequest
            {
                Symbol=symbol,
                StartTime = startTime,
                EndTime= endTime,
                Interval= BinanceExchange.API.Enums.KlineInterval.OneDay
            };
            var result = await _client.GetKlinesCandlesticks(request);
            return result.SingleOrDefault();
        }
    }
}