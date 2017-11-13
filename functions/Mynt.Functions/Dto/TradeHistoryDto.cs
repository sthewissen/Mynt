using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Functions.Dto
{
    public class TradeHistoryDto
    {
        public List<TradeDto> Trades { get; set; }
        public double TotalProfit { get; set; }
        public double TodaysProfit { get; set; }
        public double OverallBalance { get; set; }
    }
}
