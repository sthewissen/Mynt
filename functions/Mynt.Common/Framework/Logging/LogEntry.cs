using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mynt.Common.Framework.Logging
{
    public class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string Namespace { get; set; }
    }
}