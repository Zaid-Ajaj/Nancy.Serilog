using Nancy;
using Serilog;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Example
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

            Post("/echo", async args =>
            {
                await Task.Delay(1000);
                using (var reader = new StreamReader(this.Request.Body))
                {
                    var content = await reader.ReadToEndAsync();
                    return Response.AsText(content, "text/plain", Encoding.UTF8);
                }
            });
        }
    }
}