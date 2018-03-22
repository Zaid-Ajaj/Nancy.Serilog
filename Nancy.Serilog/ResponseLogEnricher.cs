using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Nancy.Serilog
{
    public class ResponseLogEnricher : ILogEventEnricher
    {
        private ResponseLogData response;
        private NancySerilogOptions options;
        public ResponseLogEnricher(ResponseLogData response, NancySerilogOptions options)
        {
            this.response = response;
            this.options = options;
            if (this.options.IgnoredResponseLogFields == null)
            {
                this.options.IgnoredResponseLogFields = new FieldChooser<ResponseLogData>();
            }
        }
         
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var ignoredFields = options.IgnoredResponseLogFields.ToArray();
            var log = response;
            
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.RequestId), new ScalarValue(log.RequestId)));
            
            
            
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.StatusCode), new ScalarValue(log.StatusCode)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.ResponseContentType), new ScalarValue(log.ResponseContentType)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.Duration), new ScalarValue(log.Duration)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.Method), new ScalarValue(log.Method)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.ReasonPhrase), new ScalarValue(log.ReasonPhrase)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.ResolvedPath), new ScalarValue(log.ResolvedPath)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.RequestedPath), new ScalarValue(log.RequestedPath)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.ResponseHeaders), EnricherProps.FromDictionary(log.ResponseHeaders)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.ResponseContent), new ScalarValue(log.ResponseContent)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.ResponseContentLength), new ScalarValue(log.ResponseContentLength)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.RawResponseCookies), EnricherProps.FromCookies(log.RawResponseCookies)));
			logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.ResponseCookies), EnricherProps.FromDictionary(log.ResponseCookies)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(log.ResolvedRouteParameters), EnricherProps.FromDictionary(log.ResolvedRouteParameters)));


            foreach(var ignoredField in ignoredFields)
            {
                logEvent.RemovePropertyIfPresent(ignoredField);
            }
        }
    }
}