using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Enums;

namespace Mynt.Core.Models
{
    public interface ITradeAdvice
    {
        TradeAdvice TradeAdvice { get; }

        double Price { get; }
    }
}
