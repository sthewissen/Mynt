using System;

namespace Mynt.Core.Api.Bittrex.Models
{
    public class Candle
    {
        public DateTime T { get; set; }
        public double H { get; set; }
        public double L { get; set; }
        public double O { get; set; }
        public double C { get; set; }
        public double V { get; set; }
        public double BV { get; set; }
    }
}
