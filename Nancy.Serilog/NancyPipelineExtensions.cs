using Nancy.Bootstrapper;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Nancy.Serilog
{
    public static class NancyPipelineExtensions
    {
        private static bool serilogEnabled = false;
        public static void EnableSerilog(this IPipelines pipelines)
        {
            if (serilogEnabled) return;

            pipelines.BeforeRequest.AddItemToStartOfPipeline(BeforePipelineHook);
            pipelines.OnError.AddItemToEndOfPipeline(OnErrorHook);
            pipelines.AfterRequest.AddItemToEndOfPipeline(AfterPipelineHook);

            serilogEnabled = true;
        } 

        static dynamic OnErrorHook(NancyContext context, Exception ex)
        {
            if (!context.Items.ContainsKey("RequestId") || !context.Items.ContainsKey("Stopwatch"))
            {
                return null;
            }

            var stopwatch = (Stopwatch)context.Items["Stopwatch"];
            stopwatch.Stop();
            var requestId = (string)context.Items["RequestId"];

            var errorLogData = new ErrorLogData();
            errorLogData.RequestId = requestId;
            errorLogData.Duration = stopwatch.ElapsedMilliseconds;
            errorLogData.ResolvedPath = context.ResolvedRoute.Description.Path;
            errorLogData.RequestedPath = context.Request.Path;
            var logger = Log.ForContext(new ErrorLogEnricher(errorLogData)); 
            logger.Error(ex, "Server Error");
            return null;
        }

        static Response BeforePipelineHook(NancyContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var nancyRequest = context.Request.ReadRequestProperties();
            nancyRequest.RequestId = Guid.NewGuid().ToString();
            context.Items.Add("Stopwatch", stopwatch);
            context.Items.Add("RequestId", nancyRequest.RequestId);
            var logger = Log.ForContext(new RequestLogEnricher(nancyRequest));
            logger.Information($"Request {nancyRequest.Method} {nancyRequest.Path.TrimAt(40)}");
            return null;
        }

        static string TrimAt(this string input, int n)
        {
            return new string(input.Take(n).ToArray());
        }

        static void AfterPipelineHook(NancyContext context)
        {
            if (!context.Items.ContainsKey("Stopwatch"))
            {
                return; 
            }

            var nancyResponseData = new ResponseLogData();
            var stopwatch = (Stopwatch)context.Items["Stopwatch"];
            stopwatch.Stop();

            nancyResponseData.RequestId = (string)context.Items["RequestId"];
            nancyResponseData.Duration = stopwatch.ElapsedMilliseconds;
            nancyResponseData.StatusCode = (int)context.Response.StatusCode;
            nancyResponseData.ResponseHeaders = new Dictionary<string, string>(context.Response.Headers);
            nancyResponseData.ResponseContentType = context.Response.ContentType ?? "";
            nancyResponseData.ReasonPhrase = context.Response.ReasonPhrase ?? "";
            nancyResponseData.ResolvedPath = context.ResolvedRoute.Description.Path;
            nancyResponseData.RequestedPath = context.Request.Path;
            nancyResponseData.RawResponseCookies = context.Response.Cookies.Select(cookie => new ResponseCookie
            {
                HttpOnly = cookie.HttpOnly,
                Secure = cookie.Secure,
                Expires = cookie.Expires,
                Name = cookie.Name,
                Value = cookie.Value
                              
            }).ToArray();

            var cookieDict = new Dictionary<string, string>();
            foreach(var cookie in context.Response.Cookies) {
                cookieDict.Add(cookie.Name, cookie.Value);
            }

            nancyResponseData.ResponseCookies = cookieDict;

            using (var memoryStream = new MemoryStream())
            {
                context.Response.Contents(memoryStream);
                memoryStream.Flush();
                memoryStream.Position = 0;
                nancyResponseData.ResponseContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                nancyResponseData.ResponseContentLength = nancyResponseData.ResponseContent.Length;
            }

            var method = context.Request.Method;
            var logger = Log.ForContext(new ResponseLogEnricher(nancyResponseData));
            logger.Information($"Response {method} {nancyResponseData.RequestedPath.TrimAt(40)}");
        }
    }
}
