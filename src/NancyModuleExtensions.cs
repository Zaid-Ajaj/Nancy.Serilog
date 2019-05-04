using Serilog;

namespace Nancy.Serilog
{
    public static class NancyModuleExtensions
    {
        public static ILogger CreateLogger(this INancyModule module)
        {
            if (!module.Context.Items.ContainsKey("RequestId"))
            {
                return Log.Logger;
            }

            var requestId = (string)module.Context.Items["RequestId"];
            var contextLogger = Log.ForContext("RequestId", requestId);
            return contextLogger;
        }
    }
}
