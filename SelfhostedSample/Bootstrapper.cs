using System;
using Nancy;
using Nancy.TinyIoc;
using Nancy.Bootstrapper;
using Nancy.Serilog;

namespace SelfhostedSample
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.EnableSerilog(new Nancy.Serilog.Options
            {
                IgnoredRequestLogFields = 
                    Ignore.FromRequest()
                        .Field(req => req.Path)
                        .Field(req => req.RequestCookies)
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
}