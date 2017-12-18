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
using System.Threading.Tasks;

namespace SelfhostedSample
{
    public class HomeModule : NancyModule
    {
        static Random rnd = new Random();

        public HomeModule()
        {
            Get["/"] = args => 
            {
                return $"{Thread.CurrentThread.ManagedThreadId}";
            };

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
                return Negotiate.WithHeader("SpecialHeader", "SpecialValue")
                                .WithCookie(new NancyCookie("special-cookie", "cookie value", DateTime.Now.AddHours(1)))
                                .WithModel($"Hello {user}");
            };
        }
    }

    class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.EnableSerilog(new Options
            {
                IgnoredRequestLogFields = 
                    Ignore.FromRequest()
                          .Field(req => req.Path)
                          .Field(req => req.RequestCookies)
                          .Field(req => req.RequestBodyContent)
                          .Field(req => req.RequestContentLength)
                          .Field(req => req.RequestContentType)
                          .Field(req => req.Query)
                          .Field(req => req.UserIPAddress),

                IgnoreErrorLogFields = 
                    Ignore.FromError()
                          .Field(error => error.ResolvedRouteParameters)
                          .Field(error => error.StatusCode),

                IgnoredResponseLogFields = 
                    Ignore.FromResponse()
                          .Field(res => res.RawResponseCookies)
                          .Field(res => res.ResponseCookies)
            });

            StaticConfiguration.DisableErrorTraces = false;
        }
    }

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
            using (var host = new NancyHost(new Uri(url), new CustomBootstrapper(), config))
            {
                host.Start();
                Console.WriteLine($"Started server at: {url}");
                Thread.Sleep(-1); // infinitely
            }
        }
    }
}
