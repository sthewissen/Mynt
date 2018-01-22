using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Interfaces;
using Mynt.Core.Strategies;

namespace Mynt.BackTester.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var backTester = new BackTester(GetTradingStrategies());
            try
            {
                backTester.WriteIntro();
                System.Console.WriteLine();
                System.Console.WriteLine();
                backTester.PresentMenuToUser();
            }
            catch (Exception ex)
            {
                backTester.WriteColoredLine($"\t{ex.Message}", ConsoleColor.Red);
                System.Console.ReadLine();
            }
        }

        private static List<ITradingStrategy> GetTradingStrategies()
        {
 return new List<ITradingStrategy>()
        {
            // The strategies we want to backtest.
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
