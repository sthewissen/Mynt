using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Models
{
    public class Ticker
    {
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Last { get; set; }
        public double Volume { get; set; }
    }
}
