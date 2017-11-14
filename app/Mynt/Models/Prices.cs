using System;
namespace Mynt.Models
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
