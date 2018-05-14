using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Enums
{
    public enum SellType
    {
        None = 0,
        StopLoss = 1,
        TrailingStopLoss = 2,
        TrailingStopLossUpdated = 3,
        Timed = 4,
        Immediate = 5,
        Strategy = 6,
        Cancelled = 7
    }
}
