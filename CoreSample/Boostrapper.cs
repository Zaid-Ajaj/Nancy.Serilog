using Nancy;
using Nancy.Serilog;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

using System;
using System.Linq;
using System.Collections.Generic;

using Serilog;
using Serilog.Formatting.Json;

namespace CoreSample
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
        
        public override void Configure(Nancy.Configuration.INancyEnvironment environment)
        {
            environment.Tracing(enabled: false, displayErrorTraces: true);
        }
    }
}