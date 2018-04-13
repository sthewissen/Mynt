using System;
using System.Collections.Generic;
using System.Text;

namespace Mynt.BackTester.Models
{
    internal class BackTestResult
    {
        public string Currency { get; set; }
        public double Profit { get; set; }
        public double Duration { get; set; }
    }
}
