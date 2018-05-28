﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Enums;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class FreqTradeClassic : BaseStrategy
    {
        public override string Name => "FreqTrade Classic";
        public override int MinimumAmountOfCandles => 100;
        public override Period IdealPeriod => Period.Hour;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var sma = candles.Sma(100);
            var closes = candles.Select(x => x.Close).ToList();
            var adx = candles.Adx();
            var tema = candles.Tema(4);
            var mfi = candles.Mfi(14);
            var sar = candles.Sar(0.02, 0.22);

            var cci = candles.Cci(5);
            var stoch = candles.StochFast();
            var bbandsLower = candles.Bbands().MiddleBand;
            var fishers = candles.Fisher();

            for (int i = 0; i < candles.Count; i++)
            {
                if (closes[i] < sma[i] && cci[i] < -100 && stoch.D[i] < 20 && fishers[i] < 0 &&
                    adx[i] > 20 && mfi[i] < 30 && tema[i] <= bbandsLower[i])
                    result.Add(TradeAdvice.Buy);
                else if (fishers[i] == 1)
                    result.Add(TradeAdvice.Sell);
                else
                    result.Add(TradeAdvice.Hold);
            }

            return result;
        }

        public override Candle GetSignalCandle(List<Candle> candles)
        {
            return candles.Last();
        }

        public override TradeAdvice Forecast(List<Candle> candles)
        {
            return Prepare(candles).LastOrDefault();
        }
    }
}
