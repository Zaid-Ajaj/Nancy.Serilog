using Nancy;
using Nancy.Serilog;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

using System;
using System.Linq;
using System.Collections.Generic;

using Serilog;
using Serilog.Formatting.Json;

namespace Example
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.EnableSerilog();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.Console(new JsonFormatter())
                .CreateLogger();
        }
        
        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            container.Register((tinyIoc, namedParams) => context.GetContextualLogger());
        }

        public override void Configure(Nancy.Configuration.INancyEnvironment environment)
        {
            environment.Tracing(enabled: false, displayErrorTraces: true);
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
}