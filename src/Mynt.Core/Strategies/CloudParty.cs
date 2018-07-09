using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Indicators;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
    public class CloudParty: BaseStrategy, INotificationTradingStrategy
    {
        private string _buymessage;

        public override string Name => "Cloud Party";
        public override int MinimumAmountOfCandles => 200;
        public override Period IdealPeriod => Period.FourHours;

        public string BuyMessage => _buymessage;
        public string SellMessage => string.Empty;

        public override List<TradeAdvice> Prepare(List<Candle> candles)
        {
            var result = new List<TradeAdvice>();

            var cloud = candles.Ichimoku(20, 60, 120, 60);
            var tkCross = cloud.TenkanSen.Crossover(cloud.KijunSen);

            for (int i = 0; i < candles.Count; i++)
            {
                // A TK cross occurred.
                if (tkCross[i])
                {
                    _buymessage = $"Tenkan crossed Kijun";

                    if (cloud.SenkouSpanA[i] > cloud.SenkouSpanB[i])
                    {
                        if (cloud.KijunSen[i] <= cloud.SenkouSpanA[i] && cloud.KijunSen[i] >= cloud.SenkouSpanB[i]) _buymessage += " - in cloud.";
                        if (cloud.KijunSen[i] > cloud.SenkouSpanA[i]) _buymessage += " - above cloud.";
                        if (cloud.KijunSen[i] < cloud.SenkouSpanB[i]) _buymessage += " - below cloud.";
                    }
                    else if (cloud.SenkouSpanA[i] < cloud.SenkouSpanB[i])
                    {
                        if (cloud.KijunSen[i] >= cloud.SenkouSpanA[i] && cloud.KijunSen[i] <= cloud.SenkouSpanB[i]) _buymessage += " - in cloud.";
                        if (cloud.KijunSen[i] > cloud.SenkouSpanB[i]) _buymessage += " - above cloud.";
                        if (cloud.KijunSen[i] < cloud.SenkouSpanA[i]) _buymessage += " - below cloud.";
                    }

                    result.Add(TradeAdvice.Buy);
                }
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
