using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Core.Backtester;
using Mynt.Core.Models;

namespace Mynt.Core.Interfaces
{
    public interface IDataStoreBacktest
    {
        Task<List<Candle>> GetBacktestCandlesBetweenTime(BacktestOptions backtestOptions);
        Task<Candle> GetBacktestLastCandle(BacktestOptions backtestOptions);
        Task<Candle> GetBacktestFirstCandle(BacktestOptions backtestOptions);
        Task SaveBacktestCandlesBulk(List<Candle> candles, BacktestOptions backtestOptions);
        Task SaveBacktestCandlesBulkCheckExisting(List<Candle> candles, BacktestOptions backtestOptions);
        Task SaveBacktestCandle(Candle candles, BacktestOptions backtestOptions);
        Task<List<string>> GetBacktestAllDatabases(BacktestOptions backtestOptions);
        Task DeleteBacktestDatabase(BacktestOptions backtestOptions);
    }
}