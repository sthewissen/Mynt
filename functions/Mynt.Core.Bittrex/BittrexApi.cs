using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mynt.Core.Api;
using Mynt.Core.Bittrex.Models;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Models;

namespace Mynt.Core.Bittrex
{
    public class BittrexApi : IExchangeApi
    {
        private BittrexClient _api;
        private readonly bool _dryRun;

        public BittrexApi(bool dryrun = true)
        {
            _dryRun = dryrun;
            _api = new BittrexClient(Constants.BittrexApiKey, Constants.BittrexApiSecret);
            Task.Factory.StartNew(CheckMarketExistance);
        }

        private async Task CheckMarketExistance()
        {
            var markets = await GetMarkets();

            foreach (var market in Constants.MarketBlackList)
            {
                if (!markets.Select(x=>x.MarketName).Contains(market))
                    throw new Exception($"Pair {market} is not available at Bittrex");
            }
        }
        
        public async Task<string> Buy(string market, double quantity, double rate)
        {
            if (_dryRun) return "DRY_RUN_BUY";

            var result = await _api.BuyLimit(market, quantity, rate);

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result.Uuid.ToString();
        }

        public async Task<string> Sell(string market, double quantity, double rate)
        {
            if (_dryRun) return "DRY_RUN_SELL";

            var result = await _api.SellLimit(market, quantity, rate);

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result.Uuid.ToString();
        }

        public async Task CancelOrder(string orderId, string market)
        {
            if (_dryRun) return;

            var result = await _api.CancelOrder(new Guid(orderId));

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");
        }

        public async Task<Core.Models.AccountBalance> GetBalance(string currency)
        {
            if (_dryRun) return new Core.Models.AccountBalance(currency, 999.99, 0);

            var result = await _api.GetBalance(currency);

            if(!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return new Core.Models.AccountBalance(result.Result.Currency, result.Result.Available, result.Result.Pending);
        }

        public async Task<List<Market>> GetMarkets()
        {
            var result = await _api.GetMarkets();

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result;
        }

        public async Task<List<Core.Models.MarketSummary>> GetMarketSummaries()
        {
            var result = await _api.GetMarketSummaries();

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result.Select(_ =>
                new Core.Models.MarketSummary
                {
                    Ask = _.Ask,
                    BaseVolume = _.BaseVolume,
                    Bid = _.Bid,
                    High = _.High,
                    Last = _.Last,
                    Low = _.Low,
                    MarketName = _.MarketName,
                    CurrencyPair = new CurrencyPair() { BaseCurrency = _.MarketName.Split('_')[1], QuoteCurrency = _.MarketName.Split('_')[0] },
                    PrevDay = _.PrevDay,
                    Volume = _.Volume
                }).ToList();
        }

        public async Task<Order> GetOrder(string orderId, string market)
        {
            if (_dryRun) return new Order();

            var result = await _api.GetOrder(new Guid(orderId));

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return new Order
            {
                ExecutedQuantity = result.Result.Quantity - result.Result.QuantityRemaining,
                OrderId = result.Result.OrderUuid.ToString(),
                OriginalQuantity = result.Result.Quantity,
                Price = result.Result.Price,
                Status = (OrderStatus) (-1), // Not supported yet.
                Side = result.Result.OrderType.ToOrderSide(),
                StopPrice = result.Result.Limit,
                Symbol = result.Result.Exchange,
                Time = result.Result.TimeStamp,
                TimeInForce = result.Result.ImmediateOrCancel ? Enums.TimeInForce.ImmediateOrCancel : Enums.TimeInForce.GoodTilCanceled,
                Type = result.Result.OrderType.ToOrderType()
            };
        }

        public async Task<List<Core.Models.OpenOrder>> GetOpenOrders(string market)
        {
            if (_dryRun) return new List<Core.Models.OpenOrder>();

            var result = await _api.GetOpenOrders(market);

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result.Select(_ =>
                new Core.Models.OpenOrder
                {
                    CancelInitiated = _.CancelInitiated,
                    Exchange = _.Exchange,
                    ImmediateOrCancel = _.ImmediateOrCancel,
                    Limit = _.Limit,
                    OrderType = _.OrderType,
                    OrderUuid = _.OrderUuid.ToString(),
                    Price = _.Price,
                    PricePerUnit = _.PricePerUnit,
                    Quantity = _.Quantity,
                    QuantityRemaining = _.QuantityRemaining
                }).ToList();
        }

        public string GetPairDetailUrl(string market)
        {
            return $"https://bittrex.com/Market/Index?MarketName={market}";
        }

        public async Task<Core.Models.Ticker> GetTicker(string market)
        {
            var result = await _api.GetTicker(market);

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return new Core.Models.Ticker
            {
                Ask = result.Result.Ask,
                Bid = result.Result.Bid,
                Last = result.Result.Last
            };
        }
        
        public async Task<HistoricAccountOrder> GetOrder(string orderId)
        {
            if(_dryRun) return new HistoricAccountOrder();

            var result = await _api.GetOrder(new Guid(orderId));

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result;
        }

        public async Task<List<Core.Models.Candle>> GetTickerHistory(string market, DateTime startDate, Core.Models.Period period = Core.Models.Period.FiveMinutes)
        {
            var result = await _api.GetTickerHistory(market, startDate.ToUnixTimestamp(), period.ToBittrexEquivalent());

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result.ToGenericCandles();
        }        
    }
}
