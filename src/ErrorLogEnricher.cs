using System;
using Serilog.Core;
using Serilog.Events;


namespace Nancy.Serilog
{
    public class ErrorLogEnricher : ILogEventEnricher
    {
        private ErrorLogData errorLog;
        private NancySerilogOptions options; 

        public ErrorLogEnricher(ErrorLogData errorLog, NancySerilogOptions options)
        {
            this.errorLog = errorLog;
            this.options = options;
            if (this.options.IgnoreErrorLogFields == null)
            {
                this.options.IgnoreErrorLogFields = new FieldChooser<ErrorLogData>();
            }
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

            foreach (var ignoreField in options.IgnoreErrorLogFields.ToArray())
            {
                logEvent.RemovePropertyIfPresent(ignoreField);
            }
        }
    }
}
