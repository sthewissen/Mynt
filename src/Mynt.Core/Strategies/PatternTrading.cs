using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Mynt.Core.Indicators;
using Mynt.Core.Models;

namespace Mynt.Core.Strategies
{
	public class PatternTrading : BaseStrategy
	{
		public override string Name => "Pattern Trading";
		public override int MinimumAmountOfCandles => 80;
		public override Period IdealPeriod => Period.Hour;

		private List<CandlePattern> _bearishPatterns = new List<CandlePattern> { CandlePattern.BearishHarami, CandlePattern.BearishKicker, CandlePattern.BearishMarubozu, CandlePattern.BearishEngulfing, CandlePattern.BearishHangingMan, CandlePattern.BearishEveningStar, CandlePattern.BearishDarkCloudCover, CandlePattern.BearishInvertedHammer };
		private List<CandlePattern> _bullishPatterns = new List<CandlePattern> { CandlePattern.BullishBelt, CandlePattern.BullishHammer, CandlePattern.BullishHarami, CandlePattern.BullishKicker, CandlePattern.BullishEngulfing, CandlePattern.BullishMarubozu, CandlePattern.BullishMorningStar };

		public override List<TradeAdvice> Prepare(List<Candle> candles)
		{
			var result = new List<TradeAdvice>();

			var patterns = candles.CandlePatterns();
			var close = candles.Close();
			var open = candles.Open();

			for (int i = 0; i < candles.Count; i++)
			{
				if (i == 0)
				{
					result.Add(TradeAdvice.Hold);
				}
				else if (patterns[i - 1].HasValue)
				{
					if (_bullishPatterns.Contains(patterns[i - 1].Value) && open[i] >= close[i - 1])
					{
						result.Add(TradeAdvice.Buy);
					}
					else if (_bearishPatterns.Contains(patterns[i - 1].Value) && close[i - 1] < open[i])
					{
						result.Add(TradeAdvice.Sell);
					}
				}
				else
				{
					result.Add(TradeAdvice.Hold);
				}
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
