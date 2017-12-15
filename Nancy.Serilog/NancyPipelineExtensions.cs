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
            errorLogData.Method = context.Request.Method;
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
            responseLogData.RawResponseCookies = context.Response.Cookies.Select(cookie => new ResponseCookie
            {
                HttpOnly = cookie.HttpOnly, 
                Secure = cookie.Secure,
                Expires = cookie.Expires,
                Name = cookie.Name,
                Value = cookie.Value
            }).ToArray();

            // Add cookies as key-valued dict, easier to index on documents
            // for example if you are using Elasticseach
            var cookieDict = new Dictionary<string, string>();
            foreach(var cookie in context.Response.Cookies) 
            {
                cookieDict.Add(cookie.Name, cookie.Value);
            }

            responseLogData.ResponseCookies = cookieDict;

            // Read the contents of the response stream and add it to log
            using (var memoryStream = new MemoryStream())
            {
                context.Response.Contents(memoryStream);
                memoryStream.Flush();
                memoryStream.Position = 0;
                responseLogData.ResponseContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                responseLogData.ResponseContentLength = responseLogData.ResponseContent.Length;
            }

            var method = context.Request.Method;
            var logger = Log.ForContext(new ResponseLogEnricher(responseLogData));
            logger.Information($"Response {method} {responseLogData.RequestedPath.TrimAt(40)}");
        }
    }
}
