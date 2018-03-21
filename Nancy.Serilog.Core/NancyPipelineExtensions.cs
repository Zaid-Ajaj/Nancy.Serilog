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

        private static Options options = new Options();

        public static void EnableSerilog(this IPipelines pipelines)
        {
            if (serilogEnabled) return;

            pipelines.BeforeRequest.AddItemToStartOfPipeline(BeforePipelineHook);
            pipelines.OnError.AddItemToEndOfPipeline(OnErrorHook);
            pipelines.AfterRequest.AddItemToEndOfPipeline(AfterPipelineHook);

            serilogEnabled = true;
        }

        public static void EnableSerilog(this IPipelines pipelines, Options options)
        {
            if (serilogEnabled) return;

            NancyPipelineExtensions.options = options;

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
            errorLogData.ResolvedRouteParameters = NancyContextExtensions.ReadDynamicDictionary(context.Parameters);
            var logger = Log.ForContext(new ErrorLogEnricher(errorLogData, options)); 
            logger.Error(ex, "Server Error at {Method} {Path}", errorLogData.Method, errorLogData.RequestedPath);
            return null;
        }

        static Response BeforePipelineHook(NancyContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestLogData = context.ReadRequestProperties(options);
            requestLogData.RequestId = Guid.NewGuid().ToString();
            context.Items.Add("Stopwatch", stopwatch);
            context.Items.Add("RequestId", requestLogData.RequestId);
            var logger = Log.ForContext(new RequestLogEnricher(requestLogData, options));
            logger.Information("Request {Method} {Path}", requestLogData.Method, requestLogData.Path);
            return null;
        }

        static void AfterPipelineHook(NancyContext context)
        {
            if (!context.Items.ContainsKey("Stopwatch"))
            {
                return; 
            }

            var responseLogData = context.ReadResponseProperties(options);
            var method = context.Request.Method;
            var logger = Log.ForContext(new ResponseLogEnricher(responseLogData, options));
            logger.Information("Response [{StatusCode}] {Method} {Path} took {Duration} ms", method, responseLogData.StatusCode, responseLogData.RequestedPath, responseLogData.Duration);
        }
    }
}
