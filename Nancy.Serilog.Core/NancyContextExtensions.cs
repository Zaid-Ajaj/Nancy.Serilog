using Nancy.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using Newtonsoft.Json;

namespace Nancy.Serilog
{
    public static class NancyContextExtensions
    {
        public static string ReadBodyContent(this RequestStream inputStream)
        {
            var content = "";
            using (var streamReader = new StreamReader(inputStream))
            {
                content = streamReader.ReadToEnd();
                // rewind stream to make it readable again from a Nancy module
                streamReader.DiscardBufferedData();
                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
            }

            return content;
        }

        /// <summary>
        /// Returns a logger with the RequestId information attached to it from the NancyContext
        /// </summary>
        /// <param name="context"></param>
        public static ILogger GetContextualLogger(this NancyContext context)
        {
            if(!context.Items.ContainsKey("RequestId"))
            {
                return Log.Logger;
            }
                
            var requestId = (string)context.Items["RequestId"];
            var contextualLogger = Log.ForContext("RequestId", requestId);
            return contextualLogger;
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
            
            IDictionary<string, string> queryDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(query.ToDictionary()));
            
            foreach (var key in queryDict.Keys)
            {
                dict.Add(key, string.Join(", ", query[key])); 
            }

            return dict; 
        }

        public static RequestLogData ReadRequestProperties(this NancyContext context, NancySerilogOptions opts)
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
                if (nancyRequest.Headers != null && nancyRequest.Headers.ContentType != null)
                {
                    request.RequestContentType = nancyRequest.Headers.ContentType.ToString();
                }
                else
                {
                    request.RequestContentType = "";
                }
            }
            
            if (!ignoredFields.Contains(nameof(request.RequestBodyContent)))
            {
                request.RequestBodyContent = "";
                
                if (!nancyRequest.Files.Any())
                {
                    // Only read request body content when there aren't any files
                    using (var streamReader = new StreamReader(nancyRequest.Body))
                    {
                        request.RequestBodyContent = streamReader.ReadToEnd();
                    }
                }
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
                if (nancyRequest.Query != null)
                {
                    request.Query = ReadDynamicDictionary(nancyRequest.Query);
                }
            }
            
            if (!ignoredFields.Contains(nameof(request.Query)))
            {
                request.RequestCookies = new Dictionary<string, string>(nancyRequest.Cookies);
            }
            
            return request;
        }


        public static ResponseLogData ReadResponseProperties(this NancyContext context, NancySerilogOptions options)
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
