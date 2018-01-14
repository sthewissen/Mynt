using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Api;
using Mynt.Core.Bittrex.Models;
using Mynt.Core.Models;
using Refit;

namespace Mynt.Core.Bittrex
{
    public class BittrexApi : IExchangeApi
    {
        private BittrexClient _api;
        private readonly bool _dryRun;

        public BittrexApi(bool dryrun = true)
        {
            _dryRun = dryrun;
            _api = new BittrexClient(Constants.BittrexApiRoot, Constants.BittrexApiKey, Constants.BittrexApiSecret);
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

        public async Task CancelOrder(Guid orderId)
        {
            if (_dryRun) return;

            var result = await _api.CancelOrder(orderId);

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");
        }

        public async Task<double> GetBalance(string currency)
        {
            if (_dryRun) return 999.99;

            var result = await _api.GetBalance(currency);

            if(!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result.Balance;
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
                    Created = _.Created,
                    High = _.High,
                    Last = _.Last,
                    Low = _.Low,
                    MarketName = _.MarketName,
                    OpenBuyOrders = _.OpenBuyOrders,
                    OpenSellOrders = _.OpenSellOrders,
                    PrevDay = _.PrevDay,
                    TimeStamp = _.TimeStamp,
                    Volume = _.Volume
                }).ToList();
        }

        public async Task<List<OpenOrder>> GetOpenOrders(string market)
        {
            if (_dryRun) return new List<OpenOrder>();

            var result = await _api.GetOpenOrders(market);

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result;
        }

        public string GetPairDetailUrl(string market)
        {
            return $"https://bittrex.com/Market/Index?MarketName={market}";
        }

        public async Task<Ticker> GetTicker(string market)
        {
            var result = await _api.GetTicker(market);

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result;
        }
        
        public async Task<HistoricAccountOrder> GetOrder(string orderId)
        {
            if(_dryRun) return new HistoricAccountOrder();

            var result = await _api.GetOrder(new Guid(orderId));

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result;
        }

        public async Task<List<Core.Models.Candle>> GetTickerHistory(string market, long startDate, Core.Models.Period period = Core.Models.Period.FiveMinutes)
        {
            var result = await _api.GetTickerHistory(market, startDate, period.ToBittrexEquivalent());

            if (!result.Success)
                throw new Exception($"Bittrex API failure {result.Message}");

            return result.Result.ToGenericCandles();
        }        
    }
}
