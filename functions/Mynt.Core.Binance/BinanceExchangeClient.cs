using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinanceExchange.API.Client;
using BinanceExchange.API.Models.Request;
using BinanceExchange.API.Models.Response;
using Mynt.Core.Binance.Models;

namespace Mynt.Core.Binance
{
    public class BinanceExchangeClient //: IExchangeClient
    {
        private readonly IBinanceClient client;

        public BinanceExchangeClient()
        {
            string apiKey = "api key";
            string secretKey = "secret key";

            // Initialise the general client client with config
            client = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = apiKey,
                SecretKey = secretKey,
            });
        }

        public async Task<UserDataStream> StartUserDataStream()
        {
            //return await client.StartUserDataStream();
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SymbolPrice>> GetSymbolsPriceTicker()
        {
            var result = await client.GetSymbolsPriceTicker();
            return result.Select(_ => new SymbolPrice { Price = _.Price, Symbol = _.Symbol });
        }

        public async Task<IEnumerable<KlineCandleStick>> GetKlinesCandlesticks(KlinesCandlesticksConfiguration configuration)
        {
            var request = new GetKlinesCandlesticksRequest
            {
                EndTime = configuration.EndTime,
                Interval = (BinanceExchange.API.Enums.KlineInterval)configuration.Interval,
                Limit = configuration.Limit,
                StartTime = configuration.StartTime,
                Symbol = configuration.Symbol
            };
            var result = await client.GetKlinesCandlesticks(request);
            return result.Select(_ => CreateKlineCandleStick(_));
        }

        public async Task<ServerTime> GetServerTime()
        {
            var result = await client.GetServerTime();
            return new ServerTime { Time = result.ServerTime };
        }
        
        public async Task<SymbolPriceChangeTicker> GetDailyTicker(string symbol)
        {
            var result = await client.GetDailyTicker(symbol);
            return CreateSymbolPriceChangeTicker(result);

        }

        private static KlineCandleStick CreateKlineCandleStick(KlineCandleStickResponse input)
        {
            var output = new KlineCandleStick
            {
                OpenTime = input.OpenTime,
                Open = input.Open,
                High = input.High,
                Low = input.Low,
                Close = input.Close,
                Volume = input.Volume,
                CloseTime = input.CloseTime,
                QuoteAssetVolume = input.QuoteAssetVolume,
                NumberOfTrades = input.NumberOfTrades,
                TakerBuyBaseAssetVolume = input.TakerBuyBaseAssetVolume,
                TakerBuyQuoteAssetVolume = input.TakerBuyQuoteAssetVolume
            };
            return output;
        }

        private static SymbolPriceChangeTicker CreateSymbolPriceChangeTicker(SymbolPriceChangeTickerResponse input)
        {
            var output = new SymbolPriceChangeTicker
            {
                PriceChange = input.PriceChange,
                PriceChangePercent = input.PriceChangePercent,
                WeightedAveragePercent = input.WeightedAveragePercent,
                PreviousClosePrice = input.PreviousClosePrice,
                LastPrice = input.LastPrice,
                BidPrice = input.BidPrice,
                AskPrice = input.AskPrice,
                OpenPrice = input.OpenPrice,
                HighPrice = input.HighPrice,
                Volume = input.Volume,
                OpenTime = input.OpenTime,
                CloseTime = input.CloseTime,
                FirstTradeId = input.FirstTradeId,
                LastId = input.LastId,
                TradeCount = input.TradeCount
            };
            return output;
        }
    }
}