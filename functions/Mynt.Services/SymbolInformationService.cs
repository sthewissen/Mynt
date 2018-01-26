using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Api;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.DataAccess.Interfaces;
using Mynt.Services.Models;

namespace Mynt.Services
{
    public class SymbolInformationService
    {
        private IExchangeApi api;

        private ICandleProvider candleProvider;

        public SymbolInformationService(IExchangeApi api, ICandleProvider candleProvider)
        {
            this.api = api;
            this.candleProvider = candleProvider;
        }

        public IEnumerable<HistoricalAdvicesModel> GetHistoricalAdvices(IEnumerable<string> symbols, ITradingStrategy strategy)
        {
            return symbols.Select(_ => CreateHistoricalAdvicesModel(_, strategy, candleProvider));
        }

        private static HistoricalAdvicesModel CreateHistoricalAdvicesModel(string symbol, ITradingStrategy strategy, ICandleProvider candleProvider)
        {
            var candles = candleProvider.GetCandles(symbol);
            var advices = strategy.Prepare(candles);
            return new HistoricalAdvicesModel
            {
                RefreshTime = DateTime.UtcNow,
                Symbol = symbol,
                Advices = advices.Select(_ => _.TradeAdvice)
            };
        }
    }
}
