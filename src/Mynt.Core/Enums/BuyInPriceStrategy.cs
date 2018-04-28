using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Enums
{
    public enum BuyInPriceStrategy
    {
        AskLastBalance = 0,
        PercentageBelowBid = 1,
        MatchCurrentBid = 2,
        SignalCandleClose = 3
    }
}
