using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Api.BlockChain.Models
{
    public class Prices
    {
        public Eur Eur { get; set; }
    }

    public class Eur
    {
        public double _15m { get; set; }
        public double Last { get; set; }
        public double Buy { get; set; }
        public double Sell { get; set; }
        public string Symbol { get; set; }
    }
}
