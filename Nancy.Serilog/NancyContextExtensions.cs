using Nancy.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Nancy.Serilog
{
    public static class NancyContextExtensions
    {
        public static string ReadBodyContent(this RequestStream inputStream)
        {
            using (var streamReader = new StreamReader(inputStream))
            {
                var content = streamReader.ReadToEnd();
                // rewind stream to make it readable again from a Nancy module
                inputStream.Position = 0;
                return content;
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

        public static RequestLogData ReadRequestProperties(this NancyContext context, Options opts)
        {
            var nancyRequest = context.Request;
            var request = new RequestLogData();
            var ignoredFields = opts.IgnoredRequestLogFields.ToArray();

            if (!ignoredFields.Contains(nameof(request.RequestHostName)))
            {
                request.RequestHostName = nancyRequest.Url.HostName;
            }

            if (!ignoredFields.Contains(nameof(request.RequestHostName)))
            {
                request.Method = nancyRequest.Method;
            }
            
            if (!ignoredFields.Contains(nameof(request.Path)))
            {
                request.Path = nancyRequest.Url.Path;
            }
            
            if (!ignoredFields.Contains(nameof(request.QueryString)))
            {
                request.QueryString = nancyRequest.Url.Query;
            }

            if (!ignoredFields.Contains(nameof(request.RequestContentLength)))
            {
                request.RequestContentLength = nancyRequest.Headers.ContentLength;
            }
            
            if (!ignoredFields.Contains(nameof(request.RequestContentType)))
            {
                request.RequestContentType = nancyRequest.Headers.ContentType;
            }
            
            if (!ignoredFields.Contains(nameof(request.RequestBodyContent)))
            {
                request.RequestBodyContent = nancyRequest.Body.ReadBodyContent();
            }
            
            if (!ignoredFields.Contains(nameof(request.RequestHeaders)))
            {
                request.RequestHeaders = nancyRequest.Headers.ReadRequestHeaders();
                if (request.RequestHeaders.ContainsKey("User-Agent"))
                {
                    try
                    {
                        var userAgent = request.RequestHeaders["User-Agent"];
                        var parser = UAParser.Parser.GetDefault();
                        var clientInfo = parser.Parse(userAgent);
                        request.UserAgentFamily = clientInfo.UserAgent.Family;
                        request.UserAgentDevice = clientInfo.Device.Family;
                        request.UserAgentOS = clientInfo.OS.Family;
                    }
                    catch
                    {
                        request.UserAgentFamily = "Other";
                        request.UserAgentDevice = "Other";
                        request.UserAgentOS = "Other";
                    }
                }
                else
                {
                    request.UserAgentFamily = "Other";
                    request.UserAgentDevice = "Other";
                    request.UserAgentOS = "Other";
                }
            }
            else
            {
                request.UserAgentFamily = "Other";
                request.UserAgentDevice = "Other";
                request.UserAgentOS = "Other";
            }
            
            if (!ignoredFields.Contains(nameof(request.UserIPAddress)))
            {
                request.UserIPAddress = nancyRequest.UserHostAddress;
            }
            
            if (!ignoredFields.Contains(nameof(request.Query)))
            {
                request.Query = ReadDynamicDictionary(nancyRequest.Query);
            }
            
            if (!ignoredFields.Contains(nameof(request.Query)))
            {
                request.RequestCookies = new Dictionary<string, string>(nancyRequest.Cookies);
            }
            
            return request;
        }


        public static ResponseLogData ReadResponseProperties(this NancyContext context, Options options)
        {
            var ignoredFields = options.IgnoredResponseLogFields.ToArray();
            
            var responseLogData = new ResponseLogData();
            var stopwatch = (Stopwatch)context.Items["Stopwatch"];
            stopwatch.Stop();

            responseLogData.RequestId = (string)context.Items["RequestId"];
            responseLogData.Duration = stopwatch.ElapsedMilliseconds;
            responseLogData.StatusCode = (int)context.Response.StatusCode;
            responseLogData.ResponseHeaders = new Dictionary<string, string>(context.Response.Headers);
            responseLogData.ResponseContentType = context.Response.ContentType ?? "";
            responseLogData.ReasonPhrase = context.Response.ReasonPhrase ?? "";
            responseLogData.ResolvedPath = context.ResolvedRoute.Description.Path;
            responseLogData.RequestedPath = context.Request.Path;
            responseLogData.Method = context.Request.Method;

            if (!ignoredFields.Contains(nameof(responseLogData.RawResponseCookies)))
            {
                var rawCookies = context.Response.Cookies.Select(cookie => new ResponseCookie
                {
                    HttpOnly = cookie.HttpOnly,
                    Secure = cookie.Secure,
                    Expires = cookie.Expires,
                    Name = cookie.Name,
                    Value = cookie.Value
                });
                
                responseLogData.RawResponseCookies = rawCookies.ToArray();
            }

            if (!ignoredFields.Contains(nameof(responseLogData.ResponseCookies)))
            {
                // Add cookies as key-valued dict, easier to index on documents
                // for example if you are using Elasticseach
                var cookieDict = new Dictionary<string, string>();
                
                foreach(var cookie in context.Response.Cookies) 
                {
                    cookieDict.Add(cookie.Name, cookie.Value);
                }

                responseLogData.ResponseCookies = cookieDict;
            }
            

            if (!ignoredFields.Contains(nameof(responseLogData.ResponseContent)))
            {
                // Read the contents of the response stream and add it to log
                using (var memoryStream = new MemoryStream())
                {
                    context.Response.Contents(memoryStream);
                    memoryStream.Flush();
                    memoryStream.Position = 0;
                    responseLogData.ResponseContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                    responseLogData.ResponseContentLength = responseLogData.ResponseContent.Length;
                }
            }
            else
            {
                responseLogData.ResponseContent = "";
                responseLogData.ResponseContentLength = 0;
            }

            if (!ignoredFields.Contains(nameof(responseLogData.ResolvedRouteParameters)))
            {
                responseLogData.ResolvedRouteParameters = ReadDynamicDictionary(context.Parameters);
            }

            return responseLogData;
        }
    }
}
