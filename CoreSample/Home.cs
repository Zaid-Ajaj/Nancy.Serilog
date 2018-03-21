using Nancy;
using Serilog;
using System.Text;
using System.Threading.Tasks;

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

            Get("/index", async args => 
            {
                await Task.Delay(500);
                return Response.AsText("<h1>Hello from Nancy</h1>", "text/html", Encoding.UTF8);
            });
        }
    }
}