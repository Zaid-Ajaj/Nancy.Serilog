using System.Collections.Generic;

namespace Nancy.Serilog
{
    public class RequestLogData : LogEntry
    {
        public string RequestHostName { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
        public string RequestBodyContent { get; set; }
        public long RequestContentLength { get; set; }
        public string RequestContentType { get; set; }
        public string UserIPAddress { get; set; }
        public string QueryString { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
    }
}
