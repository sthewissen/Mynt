using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinanceExchange.API.Client;
using BinanceExchange.API.Models.Request;
using BinanceExchange.API.Models.Response;
using Mynt.Core.Api;
using Mynt.Core.Models;

namespace Mynt.Core.Binance
{
    public class BinanceApi : IExchangeApi
    {
        private readonly BinanceClient client;

        private readonly bool isDryRunning;

        public BinanceApi(string apiKey, string secretKey, bool isDryRunning = true)
        {
            // Initialise the general client with config
            client = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = apiKey,
                SecretKey = secretKey,
            });
            this.isDryRunning = isDryRunning;
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

            if (isDryRunning)
            {
                var emptyResult = await client.CreateTestOrder(request);
                return "DRY_RUN_BUY";
            }

            var result = await client.CreateOrder(request);
            return result.ClientOrderId;
        }
        
        public async Task<string> BuyWithStopLimit(string market, double quantity, double rate, double limit)
        {
            var request = new CreateOrderRequest
            {
                Symbol = market,
                Quantity = (decimal)quantity,
                Price = rate,
                StopPrice = (decimal)limit,
                Type = BinanceExchange.API.Enums.OrderType.StopLossLimit,
                Side = BinanceExchange.API.Enums.OrderSide.Buy
            };

            if (isDryRunning)
            {
                var emptyResult = await client.CreateTestOrder(request);
                return "DRY_RUN_BUY";
            }

            var result = await client.CreateOrder(request);
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

            if (isDryRunning)
            {
                var emptyResult = await client.CreateTestOrder(request);
                return "DRY_RUN_SELL";
            }

            var result = await client.CreateOrder(request);
            return result.ClientOrderId;
        }

        public async Task<string> SellWithStopLimit(string market, double quantity, double rate, double limit)
        {
            var request = new CreateOrderRequest
            {
                Symbol = market,
                Quantity = (decimal)quantity,
                Price = rate,
                StopPrice = (decimal)limit,
                Type = BinanceExchange.API.Enums.OrderType.StopLossLimit,
                Side = BinanceExchange.API.Enums.OrderSide.Sell
            };
            
            if (isDryRunning)
            {
                var emptyResult = await client.CreateTestOrder(request);
                return "DRY_RUN_SELL";
            }

            var result = await client.CreateOrder(request);
            return result.ClientOrderId;
        }

        public async Task<AccountBalance> GetBalance(string currency)
        {
            var result = await client.GetAccountInformation();
            var currencyInformation = result.Balances.SingleOrDefault(_ => _.Asset == currency);
            return new AccountBalance(currencyInformation.Asset, (double)currencyInformation.Free, (double)currencyInformation.Locked);
        }

        public async Task<List<MarketSummary>> GetMarketSummaries()
        {
            var symbols = await client.GetSymbolsPriceTicker();
            var tasks = symbols.Select(async _ => Tuple.Create<string, SymbolPriceChangeTickerResponse, KlineCandleStickResponse>(
                _.Symbol, await client.GetDailyTicker(_.Symbol), await GetLastDaysCandle(_.Symbol)));
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

        public async Task<double> GetTotalValueInBtc()
        {
            var result = await client.GetAccountInformation();
            var total = 0.0;
            foreach (var balance in result.Balances)
            {
                if (balance.Asset.Equals("BTC", StringComparison.OrdinalIgnoreCase))
                {
                    total += (double)(balance.Free + balance.Locked);
                }
                else
                {
                    var ticket = await client.GetDailyTicker($"{balance.Asset}BTC");
                    total += (double)((balance.Free + balance.Locked) * ticket.LastPrice);
                }
            }

            return total;
        }

        public async Task<List<OpenOrder>> GetOpenOrders(string market)
        {
            var request = new CurrentOpenOrdersRequest { Symbol = market  };
            var result = await client.GetCurrentOpenOrders(request);

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

        public async Task CancelOrder(string orderId, string market)
        {
            CancelOrderRequest request = new CancelOrderRequest
            {
                Symbol = market
            };
            await client.CancelOrder(request);
        }

        public async Task<Ticker> GetTicker(string market)
        {
            var result = await client.GetDailyTicker(market);

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

                var candlesticksToAdd = await client.GetKlinesCandlesticks(request);
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
            var result = await client.GetKlinesCandlesticks(request);
            return result.SingleOrDefault();
        }
    }
}