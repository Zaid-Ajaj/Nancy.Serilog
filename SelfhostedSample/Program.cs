using System;
using System.Threading;

using Nancy;
using Nancy.Hosting.Self;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Nancy.Serilog;

using Serilog;
using Serilog.Formatting.Json;
using Nancy.Cookies;

namespace SelfhostedSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithDemystifiedStackTraces()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("ApplicationId", "SelfhostedTestApp")
                .WriteTo.Console(new JsonFormatter())
                .CreateLogger();

            var url = "http://localhost:4040";
            var config = new HostConfiguration { RewriteLocalhost = false };
            
            using (var host = new NancyHost(new Uri(url), new Bootstrapper(), config))
            {
                host.Start();
                Console.WriteLine($"Started server at: {url}");
                Thread.Sleep(-1); // infinitely
            }
        }
    }
}
