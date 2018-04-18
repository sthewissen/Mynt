using System;
using System.Collections.Generic;
using Mynt.Core.Enums;
using Mynt.Core.Models;

namespace Mynt.Core.Interfaces
{
    public interface ITradingStrategy
    {
        string Name { get;  }

        int MinimumAmountOfCandles { get; }

        Period IdealPeriod { get; }
        DateTime GetCurrentCandleDateTime();
        DateTime GetMinimumDateTime();
        DateTime GetSignalDate();

        Candle GetSignalCandle(List<Candle> candles);

        /// <summary>
        /// Gets a list of trade advices, one for each of the candles provided as input.
        /// </summary>
        /// <param name="candles">
        /// The list of candles to based the trade advices on.
        /// </param>
        /// <returns>
        /// A list of trade advices. The length of the list matches the length of the input list.
        /// </returns>
        List<TradeAdvice> Prepare(List<Candle> candles);

        /// <summary>
        /// Given a list of candles, this method forecasts what will happen next and returns 
        /// a trade advice accordingly.
        /// </summary>
        /// <param name="candles">
        /// The list of candles to based the trade advice on.
        /// </param>
        /// <returns>
        /// A trade advice based on the forecast.
        /// </returns>
        TradeAdvice Forecast(List<Candle> candles);
    }
}
