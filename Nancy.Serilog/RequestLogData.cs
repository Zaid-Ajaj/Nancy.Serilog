using System.Collections.Generic;

namespace Nancy.Serilog
{
    public class RequestLogData : LogEntry
    {
        public string RequestHostName { get; set; } = "";
        public string Path { get; set; } = "";
        public string Method { get; set; } = "";
        public string RequestBodyContent { get; set; } = "";
        public long RequestContentLength { get; set; } = 0;
        public string RequestContentType { get; set; } = "";
        public string UserIPAddress { get; set; } = "";
        public string QueryString { get; set; } = "";
        public string UserAgentFamily { get; set; } = "";
        public string UserAgentOS { get; set; } = "";
        public string UserAgentDevice { get; set; } = "";
        public Dictionary<string, string> Query { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> RequestCookies { get; set; } = new Dictionary<string, string>();
    }
}