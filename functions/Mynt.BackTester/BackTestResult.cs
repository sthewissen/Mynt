using Mynt.Core.Models;

namespace Mynt.BackTester
{
    public class BackTestResult
    {
        public string Currency { get; set; }
        public double Profit { get; set; }
        public double Duration { get; set; }
    }
}
