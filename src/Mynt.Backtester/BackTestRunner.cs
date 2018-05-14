using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mynt.Backtester.Models;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;

namespace Mynt.Backtester
{
    internal class BackTestRunner
    {
        public List<BackTestResult> RunSingleStrategy(ITradingStrategy strategy, List<string> coinsToTest, decimal stakeAmount, bool startNewTradesWhenSold)
        {
            var results = new List<BackTestResult>();

            // Go through our coinpairs and backtest them.
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 16 };
            Parallel.ForEach(coinsToTest, parallelOptions, pair =>
            {
                var candleProvider = new JsonCandleProvider("data");

                // This creates a list of buy signals.
                var candles = candleProvider.GetCandles(pair, Program.BacktestOptions.CandlePeriod);
                var backTestResult = new BackTestResult { Market = pair };

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
                                    var quantity = stakeAmount / candles[i].Close; // We always trade with 0.1 BTC.
                                    var currentProfit = (candles[j].Close - candles[i].Close) * quantity;

                                    backTestResult.Trades.Add(new BackTestTradeResult
                                    {
                                        Market = pair,
                                        Quantity = quantity,
                                        OpenRate = candles[i].Close,
                                        CloseRate = candles[j].Close,
                                        ProfitPercentage = currentProfitPercentage,
                                        Profit = currentProfit,
                                        Duration = j - i,
                                        StartDate = candles[i].Timestamp,
                                        EndDate = candles[j].Timestamp
                                    });

                                    if (startNewTradesWhenSold)
                                        i = j;

                                    break;
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Program.WriteColoredLine($"Error in Strategy: {strategy.Name}", ConsoleColor.Red);
                    Program.WriteColoredLine($"\t{ex.Message}", ConsoleColor.Red);
                }

                results.Add(backTestResult);
            });

            return results;
        }
    }
}