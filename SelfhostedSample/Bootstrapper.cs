using System;
using Nancy;
using Nancy.TinyIoc;
using Nancy.Bootstrapper;
using Nancy.Serilog;
using Serilog;

namespace SelfhostedSample
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.EnableSerilog();
            StaticConfiguration.DisableErrorTraces = false;
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            container.Register<IOtherThirdParty, OtherThirdParty>();
            container.Register<IThirdParty, ThirdParty>();
            container.Register((tinyIoc, namedParams) => context.GetContextualLogger());
        }
    }
}