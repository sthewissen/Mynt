using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Backtester.Models;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Mynt.Core.Interfaces;
using Mynt.Core.Utility;

namespace Mynt.Core.Backtester
{
    public class BackTestRunner
    {
        public async Task<List<BackTestResult>> RunSingleStrategy(ITradingStrategy strategy, BacktestOptions backtestOptions, IDataStoreBacktest dataStore)
        {
            var results = new List<BackTestResult>();

            // Go through our coinpairs and backtest them.
            foreach (string globalSymbol in backtestOptions.Coins)
            {
                var candleProvider = new DatabaseCandleProvider();
                backtestOptions.Coin = globalSymbol;

                // This creates a list of buy signals.
                var candles = await candleProvider.GetCandles(backtestOptions, dataStore);
                var backTestResult = new BackTestResult { Market = globalSymbol };

                try
                {
                    var trend = strategy.Prepare(candles);

                    for (int i = 0; i < trend.Count; i++)
                    {
                        if (trend[i] == TradeAdvice.Buy)
                        {
                            // Calculate win/lose forwards from buy point
                            for (int j = i; j < trend.Count; j++)
                            {
                                // Sell as soon as the strategy tells us to..
                                if (trend[j] == TradeAdvice.Sell)
                                {
                                    // We ignore fees for now. Goal of the backtester is to compare strategy efficiency.
                                    var currentProfitPercentage = ((candles[j].Close - candles[i].Close) / candles[i].Close) * 100;
                                    var quantity = backtestOptions.StakeAmount / candles[i].Close; // We always trade with 0.1 BTC.
                                    var currentProfit = (candles[j].Close - candles[i].Close) * quantity;

                                    backTestResult.Trades.Add(new BackTestTradeResult
                                    {
                                        Market = globalSymbol,
                                        Quantity = quantity,
                                        OpenRate = candles[i].Close,
                                        CloseRate = candles[j].Close,
                                        ProfitPercentage = currentProfitPercentage,
                                        Profit = currentProfit,
                                        Duration = j - i,
                                        StartDate = candles[i].Timestamp,
                                        EndDate = candles[j].Timestamp
                                    });

                                    if (backtestOptions.OnlyStartNewTradesWhenSold)
                                        i = j;

                                    break;
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    ConsoleUtility.WriteColoredLine($"Error in Strategy: {strategy.Name}", ConsoleColor.Red);
                    ConsoleUtility.WriteColoredLine($"\t{ex.Message}", ConsoleColor.Red);
                }

                results.Add(backTestResult);
            }

            return results;
        }
    }
}