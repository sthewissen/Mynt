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
                new AdxMomentum(),
                new AdxSmas(),
                new AwesomeMacd(),
                new AwesomeSma(),
                new Base150(),
                new BbandRsi(),
                new BigThree(),
                new BreakoutMa(),
                new BuyAndHold(),
                new CciEma(),
                new CciRsi(),
                new CciScalper(),
                new DerivativeOscillator(),
                new DoubleVolatility(),
                new EmaAdx(),
                new EmaAdxF(),
                new EmaAdxMacd(),
                new EmaAdxSmall(),
                new EmaCross(),
                new EmaStochRsi(),
                new FaMaMaMa(),
                new FifthElement(),
                new Fractals(),
                new FreqTrade(),
                new MacdSma(),
                new MacdTema(),
                new Momentum(),
                new PowerRanger(),
                new RsiBbands(),
                new RsiMacd(),
                new RsiMacdAwesome(),
                new RsiMacdMfi(),
                new RsiSarAwesome(),
                new SarAwesome(),
                new SarRsi(),
                new SarStoch(),
                new SimpleBearBull(),
                new SmaCrossover(),
                new SmaSar(),
                new SmaStochRsi(),
                new StochAdx(),
                new ThreeMAgos(),
                new TripleMa(),
                new Wvf(),
                new WvfExtended()
            };
        }
    }
}
