using System;
using System.Collections.Generic;

namespace Mynt.Models
{
    public class Installation
    {
        public string InstallationId { get; set; }
        public string Platform { get; set; }
        public string PushChannel { get; set; }
        public List<string> Tags { get; set; }
    }
}
