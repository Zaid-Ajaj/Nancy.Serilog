using System.Collections.Generic;

namespace Nancy.Serilog
{
    public class ResponseLogData : LogEntry
    {
        public int StatusCode { get; set; }
        public long Duration { get; set; }
        public string ResponseContentType { get; set; }
        public string ReasonPhrase { get; set; }
        public string ResolvedPath { get; set; }
        public Dictionary<string, string> ResponseHeaders { get; set; }
    }
}
