using System;
using Serilog.Core;
using Serilog.Events;


namespace Nancy.Serilog
{
    public class ErrorLogEnricher : ILogEventEnricher
    {
        private ErrorLogData errorLog;

        public ErrorLogEnricher(ErrorLogData errorLog)
        {
            this.errorLog = errorLog;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(errorLog.Duration), new ScalarValue(errorLog.Duration)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(errorLog.StatusCode), new ScalarValue(errorLog.StatusCode)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(errorLog.RequestId), new ScalarValue(errorLog.RequestId)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(errorLog.ResolvedPath), new ScalarValue(errorLog.ResolvedPath)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(errorLog.RequestedPath), new ScalarValue(errorLog.RequestedPath)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(errorLog.Method), new ScalarValue(errorLog.Method)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(errorLog.ResolvedRouteParameters), EnricherProps.FromDictionary(errorLog.ResolvedRouteParameters)));
        }
    }
}
