using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Binance.Models
{
    public class SymbolPriceChangeTicker
    {
        public decimal PriceChange { get; set; }

        public decimal PriceChangePercent { get; set; }

        public decimal WeightedAveragePercent { get; set; }

        public decimal PreviousClosePrice { get; set; }

        public decimal LastPrice { get; set; }

        public decimal BidPrice { get; set; }

        public decimal AskPrice { get; set; }

        public decimal OpenPrice { get; set; }

        public decimal HighPrice { get; set; }

        public decimal Volume { get; set; }

        public DateTime OpenTime { get; set; }

        public DateTime CloseTime { get; set; }

        public long FirstTradeId { get; set; }

        public long LastId { get; set; }

        public int TradeCount { get; set; }
    }
}
