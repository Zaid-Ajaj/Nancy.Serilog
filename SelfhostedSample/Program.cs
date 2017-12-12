using System;

using Nancy;
using Nancy.Hosting.Self;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Nancy.Serilog;

using Serilog;
using Serilog.Formatting.Json;

namespace SelfhostedSample
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = args => "Hello Serilog";

            Post["/hello/{user}"] = args =>
            {
                var logger = this.CreateLogger();
                var user = (string)args.user;
                if (user == "error")
                {
                    logger.Warning("Looks like an error is heading your way");
                    throw new Exception("No way!");
                }

                logger.Information("{User} Logged In", user);
                return $"Hello {user}";
            };
        }
    }

    class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.EnableSerilog();
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithDemystifiedStackTraces()
                .Enrich.WithProperty("ApplicationId", "SelfhostedTestApp")
                .WriteTo.Console(new JsonFormatter())
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            var url = "http://localhost:8080";
            var config = new HostConfiguration { RewriteLocalhost = false };
            using (var host = new NancyHost(new Uri(url), new CustomBootstrapper()))
            {
                host.Start();
                Console.WriteLine($"Started server at: {url}");
                Console.ReadKey();
            }
        }
    }
}
