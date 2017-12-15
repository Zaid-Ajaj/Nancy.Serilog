using Nancy.IO;
using System.Collections.Generic;
using System.IO;

namespace Nancy.Serilog
{
    public static class NancyContextExtensions
    {
        public static string ReadBodyContent(this RequestStream inputStream)
        {
            using (var streamReader = new StreamReader(inputStream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static Dictionary<string, string> ReadRequestHeaders(this RequestHeaders headers)
        {
            var dict = new Dictionary<string, string>();
            foreach (var key in headers.Keys)
            {
                dict.Add(key, string.Join(", ", headers[key]));
            }

            return dict;
        }

        public static Dictionary<string, string> ReadDynamicDictionary(dynamic query)
        {
            if (query == null) return new Dictionary<string, string>();

            var dict = new Dictionary<string, string>();

            foreach (var key in query.Keys)
            {
                dict.Add(key, string.Join(", ", query[key])); 
            }

            return dict; 
        }

        public static RequestLogData ReadRequestProperties(this NancyContext context)
        {
            var nancyRequest = context.Request;
            var request = new RequestLogData();
            request.RequestHostName = nancyRequest.Url.HostName;
            request.Method = nancyRequest.Method;
            request.Path = nancyRequest.Url.Path;
            request.QueryString = nancyRequest.Url.Query;
            request.RequestContentLength = nancyRequest.Headers.ContentLength;
            request.RequestContentType = nancyRequest.Headers.ContentType;
            request.RequestBodyContent = nancyRequest.Body.ReadBodyContent();
            request.RequestHeaders = nancyRequest.Headers.ReadRequestHeaders();
            request.UserIPAddress = nancyRequest.UserHostAddress;
            request.Query = ReadDynamicDictionary(nancyRequest.Query);
            request.RequestCookies = new Dictionary<string, string>(nancyRequest.Cookies);
            return request;
        }
    }
}
