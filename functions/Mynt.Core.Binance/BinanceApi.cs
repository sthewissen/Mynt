﻿using Binance.Net;
using Binance.Net.Objects;
using Mynt.Core.Api;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderSide = Binance.Net.Objects.OrderSide;
using OrderStatus = Binance.Net.Objects.OrderStatus;
using OrderType = Binance.Net.Objects.OrderType;
using TimeInForce = Binance.Net.Objects.TimeInForce;

namespace Mynt.Core.Binance
{
    public class BinanceApi : IExchangeApi
    {
        private readonly BinanceClient _client;
        private readonly bool _isDryRunning;
        private BinanceExchangeInfo _exchangeInfo;
        private readonly Constants _settings;

        public BinanceApi(Constants settings)
        {
            _settings = settings;
            // Initialise the general client with config
            _client = new BinanceClient(_settings.BinanceApiKey, _settings.BinanceApiSecret)
            {
                TradeRulesBehaviour = TradeRulesBehaviour.AutoComply,
                AutoTimestamp = true
            };

            this._isDryRunning = _settings.IsDryRunning;
        }

        #region IExchangeApi Implementations

        public async Task<string> Buy(string market, double quantity, double rate)
        {
            if (_isDryRunning)
            {
                var emptyResult = await _client.PlaceTestOrderAsync(market, OrderSide.Buy, OrderType.Limit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel);
                return $"{DateTime.UtcNow.Year}{DateTime.UtcNow.Month}{DateTime.UtcNow.Day}{DateTime.UtcNow.Hour}{DateTime.UtcNow.Minute}{DateTime.UtcNow.Second}";
            }

            var result = await _client.PlaceOrderAsync(market, OrderSide.Buy, OrderType.Limit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel);

            if (!result.Success) throw new Exception(result.Error.Message);

            return result.Data.OrderId.ToString();
        }

        public async Task<string> Sell(string market, double quantity, double rate)
        {
            if (_isDryRunning)
            {
                var emptyResult = await _client.PlaceTestOrderAsync(market, OrderSide.Sell, OrderType.Limit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel);
                return $"{DateTime.UtcNow.Year}{DateTime.UtcNow.Month}{DateTime.UtcNow.Day}{DateTime.UtcNow.Hour}{DateTime.UtcNow.Minute}{DateTime.UtcNow.Second}";
            }

            var result = await _client.PlaceOrderAsync(market, OrderSide.Sell, OrderType.Limit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel);

            if (!result.Success) throw new Exception(result.Error.Message);

            return result.Data.OrderId.ToString();
        }

        public async Task<AccountBalance> GetBalance(string currency)
        {
            var result = await _client.GetAccountInfoAsync();

            if (!result.Success) throw new Exception(result.Error.Message);

            var currencyInformation = result.Data.Balances.SingleOrDefault(_ => _.Asset == currency);
            return new AccountBalance(currencyInformation.Asset, (double)currencyInformation.Free, (double)currencyInformation.Locked);
        }

        public async Task<List<MarketSummary>> GetMarketSummaries()
        {
            var symbols = await _client.Get24HPricesListAsync();

            if (!symbols.Success) throw new Exception(symbols.Error.Message);

            var result = new List<MarketSummary>();

            foreach (var _ in symbols.Data)
            {
                var info = await GetSymbolInfo(_.Symbol);
                result.Add(new MarketSummary()
                {
                    Ask = (double)_.AskPrice,
                    BaseVolume = (double)_.QuoteVolume,
                    CurrencyPair = new CurrencyPair() { BaseCurrency = info.BaseAsset, QuoteCurrency = info.QuoteAsset },
                    Volume = (double)_.Volume,
                    MarketName = _.Symbol,
                    Bid = (double)_.BidPrice,
                    High = (double)_.HighPrice,
                    Low = (double)_.LowPrice,
                    Last = (double)_.LastPrice
                });
            }

            return result;
        }

        public async Task<Order> GetOrder(string orderId, string market)
        {
            if (_isDryRunning) return new Order { OrderId = orderId, Symbol = market, Status = Enums.OrderStatus.Canceled };

            int longId;

            if (!int.TryParse(orderId, out longId))
            {
                throw new ArgumentException("'orderId' should be of type long but cannot be parsed");
            }

            var result = await _client.GetAllOrdersAsync(market, longId);

            if (!result.Success) throw new Exception(result.Error.Message);

            if (result.Data.Any())
            {
                var order = result.Data.FirstOrDefault();

                return new Order
                {
                    ExecutedQuantity = (double)order.ExecutedQuantity,
                    OrderId = order.OrderId.ToString(),
                    OriginalQuantity = (double)order.OriginalQuantity,
                    Price = (double)order.Price,
                    Side = order.Side.ToCoreEquivalent(),
                    Status = order.Status.ToCoreEquivalent(),
                    StopPrice = (double)order.StopPrice,
                    Symbol = order.Symbol,
                    Time = order.Time,
                    TimeInForce = order.TimeInForce.ToCoreEquivalent(),
                    Type = order.Type.ToCoreEquivalent()
                };
            }

            return null;
        }

        public async Task<double> GetTotalValueInBtc()
        {
            var result = await _client.GetAccountInfoAsync();
            var total = 0.0;

            if (!result.Success) throw new Exception(result.Error.Message);

            foreach (var balance in result.Data.Balances)
            {
                if (balance.Asset.Equals("BTC", StringComparison.OrdinalIgnoreCase))
                {
                    total += (double)(balance.Free + balance.Locked);
                }
                else
                {
                    var ticket = await _client.Get24HPriceAsync($"{balance.Asset}BTC");

                    if (!ticket.Success) throw new Exception(ticket.Error.Message);

                    total += (double)((balance.Free + balance.Locked) * ticket.Data.AskPrice);
                }
            }

            return total;
        }

        public async Task<List<OpenOrder>> GetOpenOrders(string market)
        {
            var result = await _client.GetOpenOrdersAsync(market);

            if (!result.Success) throw new Exception(result.Error.Message);

            return result.Data.Select(_ =>
                new OpenOrder
                {
                    CancelInitiated = (_.Status == OrderStatus.Canceled || _.Status == OrderStatus.PendingCancel),
                    Exchange = _.Symbol,
                    ImmediateOrCancel = (_.TimeInForce == TimeInForce.ImmediateOrCancel),
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
            // Don't allow cancellation if in dry run mode.
            if (_isDryRunning) return;

            long longId;

            if (!long.TryParse(orderId, out longId))
            {
                throw new ArgumentException("'orderId' should be of type long but cannot be parsed");
            }

            await _client.CancelOrderAsync(market, longId);
        }

        public async Task<OrderBook> GetOrderBook(string symbol)
        {
            var result = await _client.GetOrderBookAsync(symbol, 1000);
            if (!result.Success) throw new Exception(result.Error.Message);

            return new OrderBook
            {
                Asks = result.Data.Asks.Select(_ => new OrderBookEntry { Price = _.Price, Quantity = _.Quantity }).ToList(),
                Bids = result.Data.Bids.Select(_ => new OrderBookEntry { Price = _.Price, Quantity = _.Quantity }).ToList(),
            };
        }

        public async Task<Ticker> GetTicker(string market)
        {
            var result = await _client.Get24HPriceAsync(market);

            if (!result.Success) throw new Exception(result.Error.Message);

            return new Ticker
            {
                Ask = (double)result.Data.AskPrice,
                Bid = (double)result.Data.BidPrice,
                Last = (double)result.Data.LastPrice,
                Volume = (double)result.Data.Volume
            };
        }

        public async Task<List<Candle>> GetTickerHistory(string market, Period period, DateTime startDate, DateTime? endDate = null)
        {
            var endTime = endDate == null? DateTime.UtcNow: endDate.Value;
            var start = startDate;
            var candles = new List<BinanceKline>();

            while (start < endTime)
            {
                var candlesticksToAdd = await _client.GetKlinesAsync(market, period.ToBinanceEquivalent(), start, endTime);

                if (!candlesticksToAdd.Success) throw new Exception(candlesticksToAdd.Error.Message);

                candles.AddRange(candlesticksToAdd.Data);
                start = !candlesticksToAdd.Data.Any() ? DateTime.MaxValue : candlesticksToAdd.Data.Max(_ => _.CloseTime);
            }

            return candles.Select(_ =>
                new Candle
                {
                    Close = (double)_.Close,
                    High = (double)_.High,
                    Low = (double)_.Low,
                    Open = (double)_.Open,
                    Timestamp = _.OpenTime
                }
            ).ToList();
        }

        public async Task<List<Candle>> GetTickerHistory(string market, Period period, int length)
        {
            var stepSize = period.ToMinutesEquivalent();
            var endTime = DateTime.UtcNow;
            var startDate = endTime.AddMinutes(-stepSize * length);
            var candles = await GetTickerHistory(market, period, startDate);

            // Add extra candles if we didn't get enough for some reason.
            while (candles.Count < length)
            {
                endTime = startDate;
                startDate = endTime.AddMinutes(stepSize * (candles.Count - length));
                var candlesticksToAdd = await GetTickerHistory(market, period, startDate, endTime);
                candles.AddRange(candlesticksToAdd);
            }

            return candles.OrderBy(_ => _.Timestamp).ToList(); ;
        }

        #endregion

        public async Task<DateTime> GetServerTime()
        {
            var result = await _client.GetServerTimeAsync();

            if (!result.Success) throw new Exception(result.Error.Message);

            return result.Data;
        }

        public async Task<BinanceSymbol> GetSymbolInfo(string symbol)
        {
            if (_exchangeInfo == null)
            {
                var result = await _client.GetExchangeInfoAsync();
                if (!result.Success) throw new Exception(result.Error.Message);
                _exchangeInfo = result.Data;
            }

            return _exchangeInfo.Symbols.FirstOrDefault(x => x.SymbolName == symbol);
        }
        
        public async Task<string> BuyWithStopLimit(string market, double quantity, double rate, double limit)
        {
            if (_isDryRunning)
            {
                var emptyResult = await _client.PlaceTestOrderAsync(market, OrderSide.Buy, OrderType.StopLossLimit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel, (decimal)limit);
                return $"{DateTime.UtcNow.Year}{DateTime.UtcNow.Month}{DateTime.UtcNow.Day}{DateTime.UtcNow.Hour}{DateTime.UtcNow.Minute}{DateTime.UtcNow.Second}";
            }

            var result = await _client.PlaceOrderAsync(market, OrderSide.Buy, OrderType.StopLossLimit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel, (decimal)limit);

            if (!result.Success) throw new Exception(result.Error.Message);

            return result.Data.OrderId.ToString();
        }

        public async Task<string> BuyWithTakeProfit(string market, double quantity, double rate, double limit)
        {
            if (_isDryRunning)
            {
                var emptyResult = await _client.PlaceTestOrderAsync(market, OrderSide.Buy, OrderType.TakeProfitLimit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel, (decimal)limit);
                return $"{DateTime.UtcNow.Year}{DateTime.UtcNow.Month}{DateTime.UtcNow.Day}{DateTime.UtcNow.Hour}{DateTime.UtcNow.Minute}{DateTime.UtcNow.Second}";
            }

            var result = await _client.PlaceOrderAsync(market, OrderSide.Buy, OrderType.TakeProfitLimit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel, (decimal)limit);

            if (!result.Success) throw new Exception(result.Error.Message);

            return result.Data.OrderId.ToString();
        }

        public async Task<string> SellWithStopLimit(string market, double quantity, double rate, double limit)
        {
            if (_isDryRunning)
            {
                var emptyResult = await _client.PlaceTestOrderAsync(market, OrderSide.Sell, OrderType.StopLossLimit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel, (decimal)limit);
                return $"{DateTime.UtcNow.Year}{DateTime.UtcNow.Month}{DateTime.UtcNow.Day}{DateTime.UtcNow.Hour}{DateTime.UtcNow.Minute}{DateTime.UtcNow.Second}";
            }

            var result = await _client.PlaceOrderAsync(market, OrderSide.Sell, OrderType.StopLossLimit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel, (decimal)limit);

            if (!result.Success) throw new Exception(result.Error.Message);

            return result.Data.OrderId.ToString();
        }

        public async Task<string> SellWithTakeProfit(string market, double quantity, double rate, double limit)
        {
            if (_isDryRunning)
            {
                var emptyResult = await _client.PlaceTestOrderAsync(market, OrderSide.Sell, OrderType.TakeProfitLimit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel, (decimal)limit);
                return $"{DateTime.UtcNow.Year}{DateTime.UtcNow.Month}{DateTime.UtcNow.Day}{DateTime.UtcNow.Hour}{DateTime.UtcNow.Minute}{DateTime.UtcNow.Second}";
            }

            var result = await _client.PlaceOrderAsync(market, OrderSide.Sell, OrderType.TakeProfitLimit, (decimal)quantity, null, (decimal)rate, TimeInForce.GoodTillCancel, (decimal)limit);

            if (!result.Success) throw new Exception(result.Error.Message);

            return result.Data.OrderId.ToString();
        }

        private async Task<BinanceKline> GetLastDaysCandle(string symbol)
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddDays(-1);
            var result = await _client.GetKlinesAsync(symbol, KlineInterval.OneDay, startTime, endTime);

            if (!result.Success) throw new Exception(result.Error.Message);

            return result.Data.SingleOrDefault();
        }
    }
}