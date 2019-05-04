using System.Collections.Generic;

namespace Nancy.Serilog
{
    public class ResponseLogData : LogEntry
    {
        public int StatusCode { get; set; } = 0;
        public long Duration { get; set; } = 0;
        public string Method { get; set; } = "";
        public string ResponseContentType { get; set; } = "";
        public string ResponseContent { get; set; } = "";
        public long ResponseContentLength { get; set; } = 0;
        public string ReasonPhrase { get; set; } = "";
        public string ResolvedPath { get; set; } = "";
        public string RequestedPath { get; set; } = "";
        public Dictionary<string, string> ResolvedRouteParameters { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ResponseHeaders { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ResponseCookies { get; set; } = new Dictionary<string, string>();
        public ResponseCookie[] RawResponseCookies { get; set; } = new ResponseCookie[] { };
    }
}