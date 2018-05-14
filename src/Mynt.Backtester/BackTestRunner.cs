using System;
using System.Collections.Generic;
using Mynt.Backtester.Models;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;

namespace Mynt.Backtester
{
    internal class BackTestRunner
    {
        public List<BackTestResult> RunSingleStrategy(ITradingStrategy strategy, List<string> coinsToTest, decimal stakeAmount)
        {
            var results = new List<BackTestResult>();

            // Go through our coinpairs and backtest them.
            foreach (var pair in coinsToTest)
            {
                var candleProvider = new JsonCandleProvider("data");

                // This creates a list of buy signals.
                var candles = candleProvider.GetCandles(pair);
                var trend = strategy.Prepare(candles);

                var backTestResult = new BackTestResult { Market = pair };

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
                                    Quatity = quantity,
                                    OpenRate = candles[i].Close,
                                    CloseRate = candles[j].Close,
                                    ProfitPercentage = currentProfitPercentage,
                                    Profit = currentProfit,
                                    Duration = j - i,
                                    StartDate = candles[i].Timestamp,
                                    EndDate = candles[j].Timestamp
                                });
                                break;
                            }
                        }
                    }
                }

                results.Add(backTestResult);
            }

            return results;
        }
    }
}