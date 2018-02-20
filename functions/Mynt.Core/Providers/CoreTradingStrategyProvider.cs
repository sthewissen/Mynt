using System.Collections.Generic;
using Mynt.Core.Interfaces;
using Mynt.Core.Strategies;

namespace Mynt.Core.Providers
{
    class CoreTradingStrategyProvider : ITradingStrategyProvider
    {
        public IEnumerable<ITradingStrategy> CreateStrategies()
        {
            return new List<ITradingStrategy>()
            {
               new BigThree(),
                new BuyAndHold(),
            new SmaCrossover(),
                new TheScalper(),
                new Wvf(),
                new WvfExtended()
            };
        }
    }
}
