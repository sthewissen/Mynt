using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Enums
{
    public enum SellType
    {
        None,
        StopLoss,
        TrailingStopLoss,
        TrailingStopLossUpdated,
        Timed,
        Immediate,
        Strategy,
        Cancelled
    }
}
