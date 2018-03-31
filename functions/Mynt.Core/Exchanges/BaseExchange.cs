using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExchangeSharp;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Exchanges
{
    public class BaseExchange : IExchangeApi
    {
        private readonly ExchangeSharp.ExchangeAPI _api;
        private readonly Exchange _exchange;
        private List<ExchangeSharp.ExchangeMarket> _exchangeInfo;

        public BaseExchange(ExchangeOptions options)
        {
            _exchange = options.Exchange;

            switch (_exchange)
            {
                case Exchange.Binance:
                    _api = new ExchangeSharp.ExchangeBinanceAPI();
                    break;
                case Exchange.Bitfinex:
                    _api = new ExchangeSharp.ExchangeBitfinexAPI();
                    break;
                case Exchange.Bittrex:
                    _api = new ExchangeSharp.ExchangeBittrexAPI();
                    break;
                case Exchange.Poloniex:
                    _api = new ExchangeSharp.ExchangePoloniexAPI();
                    break;
            }

            _api.LoadAPIKeysUnsecure(options.ApiKey, options.ApiSecret, options.PassPhrase);
        }

        public async Task<string> Buy(string market, decimal quantity, decimal rate)
        {
            var request = new ExchangeSharp.ExchangeOrderRequest()
            {
                Amount = quantity,
                IsBuy = true,
                OrderType = ExchangeSharp.OrderType.Limit,
                Price = rate,
                Symbol = market
            };

            var order = await _api.PlaceOrderAsync(request);

            return order.OrderId;
        }

        public async Task CancelOrder(string orderId, string market)
        {
            var fixedOrderId = orderId;

            // HACK: Binance cancel order request requires the market as well...
            if (_exchange == Exchange.Binance)
                fixedOrderId = market + fixedOrderId;

            await _api.CancelOrderAsync(fixedOrderId);
        }

        public async Task<AccountBalance> GetBalance(string currency)
        {
            var balances = await _api.GetAmountsAvailableToTradeAsync();

            if (balances.ContainsKey(currency))
                return new AccountBalance(currency, balances[currency], 0);
            else
                return new AccountBalance(currency, 0, 0);
        }

        public async Task<List<Models.MarketSummary>> GetMarketSummaries()
        {
            var summaries = (await _api.GetTickersAsync()).ToList();

            if (summaries.Count > 0)
            {
                var result = new List<Models.MarketSummary>();

                foreach (var summary in summaries)
                {
                    var info = await GetSymbolInfo(summary.Key);
                    result.Add(new Models.MarketSummary()
                    {
                        CurrencyPair = new CurrencyPair() { BaseCurrency = info.MarketCurrency, QuoteCurrency = info.BaseCurrency },
                        MarketName = summary.Key,
                        Ask = summary.Value.Ask,
                        Bid = summary.Value.Bid,
                        Last = summary.Value.Last,
                        Volume = summary.Value.Volume.QuantityAmount,
                    });
                }

                return result;
            }

            return new List<Models.MarketSummary>();
        }

        public async Task<List<OpenOrder>> GetOpenOrders(string market)
        {
            var orders = (await _api.GetOpenOrderDetailsAsync(market)).ToList();

            if (orders.Count > 0)
            {
                return orders.Select(x => new OpenOrder
                {
                    Exchange = _exchange.ToString(),
                    OriginalQuantity = x.Amount,
                    ExecutedQuantity = x.AmountFilled,
                    OrderId = x.OrderId,
                    Side = x.IsBuy ? OrderSide.Buy : OrderSide.Sell,
                    Market = x.Symbol,
                    Price = x.Price,
                    OrderDate = x.OrderDate,
                    Status = x.Result.ToOrderStatus()
                }).ToList();
            }

            return new List<OpenOrder>();
        }

        public async Task<Order> GetOrder(string orderId, string market)
        {
            var order = await _api.GetOrderDetailsAsync(orderId);

            if (order != null)
            {
                return new Order
                {
                    Exchange = _exchange.ToString(),
                    OriginalQuantity = order.Amount,
                    ExecutedQuantity = order.AmountFilled,
                    OrderId = order.OrderId,
                    Price = order.Price,
                    Market = order.Symbol,
                    Side = order.IsBuy ? OrderSide.Buy : OrderSide.Sell,
                    OrderDate = order.OrderDate,
                    Status = order.Result.ToOrderStatus()
                };
            }

            return null;
        }

        public async Task<OrderBook> GetOrderBook(string market)
        {
            var orderbook = await _api.GetOrderBookAsync(market);
            var orderbookFixed = new OrderBook
            {
                Asks = orderbook.Asks.Select(x => new OrderBookEntry {Price = x.Price, Quantity = x.Amount}).ToList(),
                Bids = orderbook.Bids.Select(x => new OrderBookEntry {Price = x.Price, Quantity = x.Amount}).ToList()
            };

            return orderbookFixed;
        }

        public async Task<Ticker> GetTicker(string market)
        {
            var ticker = await _api.GetTickerAsync(market);

            if (ticker != null)
                return new Ticker
                {
                    Ask = ticker.Ask,
                    Bid = ticker.Bid,
                    Last = ticker.Last,
                    Volume = ticker.Volume.QuantityAmount
                };

            return null;
        }

        public async Task<List<Candle>> GetTickerHistory(string market, Period period, DateTime startDate, DateTime? endDate = null)
        {
            var ticker = (await _api.GetCandlesAsync(market, period.ToMinutesEquivalent() * 60, startDate, endDate)).ToList();

            if (ticker.Count > 0)
                return ticker.Select(x => new Candle
                {
                    Close = x.ClosePrice,
                    High = x.HighPrice,
                    Low = x.LowPrice,
                    Open = x.OpenPrice,
                    Timestamp = x.Timestamp,
                    Volume = (decimal)x.VolumeQuantity
                }).ToList();

            return new List<Candle>();
        }

        public async Task<List<Candle>> GetTickerHistory(string market, Period period, int length)
        {
            var ticker = (await _api.GetCandlesAsync(market, period.ToMinutesEquivalent() * 60, null, null, length)).ToList();

            if (ticker.Count() > 0)
                return ticker.Select(x => new Candle
                {
                    Close = x.ClosePrice,
                    High = x.HighPrice,
                    Low = x.LowPrice,
                    Open = x.OpenPrice,
                    Timestamp = x.Timestamp,
                    Volume = (decimal)x.VolumeQuantity
                }).ToList();

            return new List<Candle>();
        }

        public async Task<string> Sell(string market, decimal quantity, decimal rate)
        {
            var request = new ExchangeSharp.ExchangeOrderRequest()
            {
                Amount = quantity,
                IsBuy = false,
                OrderType = ExchangeSharp.OrderType.Limit,
                Price = rate,
                Symbol = market
            };

            var order = await _api.PlaceOrderAsync(request);

            return order.OrderId;
        }

        public async Task<ExchangeMarket> GetSymbolInfo(string symbol)
        {
            if (_exchangeInfo == null)
            {
                var result = (await _api.GetSymbolsMetadataAsync()).ToList();
                _exchangeInfo = result;
            }

            return _exchangeInfo.FirstOrDefault(x => x.MarketName == symbol);
        }
    }
}
