using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nancy.Serilog
{
    public class ErrorLogData : LogEntry
    {
        public long Duration { get; set; }
        public string ResolvedPath { get; set; }
        public string RequestedPath { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> ResolvedRouteParameters { get; set; }
        public int StatusCode { get; set; } = 500; // Internal Server Error,
    }
}
