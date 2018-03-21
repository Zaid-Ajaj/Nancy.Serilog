using Nancy;
using Serilog;

namespace CoreSample
{
    public class Home : NancyModule
    {
        public Home(ILogger logger)
        {
            Get("/", args => "Hello From Home");
            
            Get("/other", args => 
            {
                logger.Information("Logging from /other");
                return "Hello from other";
            });
        }
    }
}