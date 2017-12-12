using Nancy.Bootstrapper;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            if (!context.Items.ContainsKey("RequestId"))
            {
                return null;
            }

            var requestId = (string)context.Items["RequestId"];
            var logger = Log.ForContext("RequestId", requestId); 
            logger.Error(ex, "Server Error");
            return null;
        }

        static Response BeforePipelineHook(NancyContext context)
        {
            var nancyRequest = context.Request.ReadRequestProperties();
            nancyRequest.RequestId = Guid.NewGuid().ToString();
            var stopwatch = Stopwatch.StartNew();
            context.Items.Add("Stopwatch", stopwatch);
            context.Items.Add("RequestId", nancyRequest.RequestId);
            var logger = Log.ForContext(new RequestLogEnricher(nancyRequest));
            logger.Information("Request Initiated", nancyRequest.RequestId);
            return null;
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
            nancyResponseData.ResponseHeaders = (Dictionary<string, string>)context.Response.Headers;
            nancyResponseData.ResponseContentType = context.Response.ContentType ?? "";
            nancyResponseData.ReasonPhrase = context.Response.ReasonPhrase ?? "";
            nancyResponseData.ResolvedPath = context.ResolvedRoute.Description.Path;
            var logger = Log.ForContext(new ResponseLogEnricher(nancyResponseData));
            logger.Information("Response Returned", nancyResponseData.RequestId);
        }
    }
}
