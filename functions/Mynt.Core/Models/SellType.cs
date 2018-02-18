using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Models
{
    public enum SellType
    {
        None,
        StopLoss,
        StopLossAnchor,
        Timed,
        Immediate,
        Strategy,
        Cancelled
    }
}
