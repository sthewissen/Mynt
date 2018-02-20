using System;
using System.Collections.Generic;
using Mynt.Core.Enums;

namespace Mynt.Services.Models
{
    public class HistoricalAdvicesModel
    {
        public DateTime RefreshTime { get; set; }

        public string Symbol { get; set; }

        public IEnumerable<TradeAdvice> Advices { get; set; }
    }
}
