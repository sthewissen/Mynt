using System;
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
    public class EmaAdx : ITradingStrategy
    {
        public string Name => "EMA ADX";
        
        public List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var emaFast = candles.Ema(12);
            var emaShort = candles.Ema(36);
            var adx = candles.Adx();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i == 0)
                    result.Add(TradeAdvice.Hold);
                else if (emaFast[i] > emaShort[i] && emaFast[i - 1] < emaShort[i] && adx[i] < 20)
                    result.Add(TradeAdvice.Buy);
                else if (emaFast[i] < emaShort[i] && emaFast[i - 1] > emaShort[i] && adx[i] >= 20)
                    result.Add(TradeAdvice.Sell);
                else
                    result.Add(TradeAdvice.Hold);
            }

            return result;
        }
    }
}
