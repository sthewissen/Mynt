using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Binance.Models
{
    public class SymbolPrice
    {
        public string Symbol { get; set; }

        public decimal Price { get; set; }
    }
}
