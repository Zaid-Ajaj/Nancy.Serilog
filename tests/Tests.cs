using System;
using System.Collections.Generic;
using TinyTest;
using Nancy;
using Nancy.Bootstrapper;
using Serilog;
using Serilog.Events;
using Nancy.Serilog;
using Nancy.Testing;
using Nancy.TinyIoc;
using Serilog.Sinks.TestCorrelator;
using System.Linq;

namespace Nancy.Serilog.Core.Tests
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.EnableSerilog();
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            container.Register((tinyIoc, namedParams) => context.GetContextualLogger());
            container.Register<IThirdParty, ThirdParty>();
        }

                // Reads nancy module information from the assembly
        protected override IEnumerable<ModuleRegistration> Modules
        {
            get
            {
                return 
                    this.GetType()
                    .Assembly
                    .GetTypes()
                    .Where(type => type.IsSubclassOf(typeof(NancyModule)))
                    .Select(module => new ModuleRegistration(module))
                    .ToList();
            }
        }
    }

    public interface IThirdParty 
    {
        void Work();
    }

    public class ThirdParty : IThirdParty
    {
        private readonly ILogger logger; 
        public ThirdParty(ILogger logger)
        {
            this.logger = logger;
        }   

        public void Work() 
        {
            logger.Information("Third party is working");
        }
    }

    public class Home : NancyModule
    {
        public Home(IThirdParty thirdParty, ILogger logger)
        {
            Get("/", args => 
            {
                logger.Information("Requesting root...");
                thirdParty.Work();
                logger.Information("Done working");
                return "Root";
            });
        }
    }

    class Tests
    {
        static void InitLogger() 
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.TestCorrelator()
                .CreateLogger();
        }

        static int Main(string[] args)
        {
            InitLogger();

            Test.Module("Nancy.Serilog.Core Tests");

            Test.CaseAsync("Serilog emits correlated log events when with same logger being passed down to dependencies", async () => 
            {
                using (TestCorrelator.CreateContext())
                {
                    var browser = new Browser(new Bootstrapper());

                    var result = await browser.Get("/");

                    var eventLogs = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                    
                    Test.Equal(5, eventLogs.Count());
                    Test.Equal(true, eventLogs.All(log => log.Level == LogEventLevel.Information));
                    
                    var firstRequestId = eventLogs.First().Properties.AsEnumerable().First(log => log.Key == "RequestId").Value.ToString();
                    Test.Equal(false, String.IsNullOrWhiteSpace(firstRequestId));

                    var allRequestIds = eventLogs.Select(log => 
                    {
                        return log.Properties
                                  .ToList()
                                  .First(prop => prop.Key == "RequestId")
                                  .Value
                                  .ToString();   
                    });

                    Test.Equal(true, allRequestIds.All(requestId => requestId == firstRequestId));
                }
            });

            return Test.Report();
        }
    }
}
