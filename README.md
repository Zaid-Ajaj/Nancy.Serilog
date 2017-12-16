# Nancy.Serilog [![Nuget](https://img.shields.io/nuget/v/Nancy.Serilog.svg?colorB=green)](https://www.nuget.org/packages/Nancy.Serilog)

[Nancy](https://github.com/NancyFx/Nancy) plugin for application-wide logging using the [Serilog](https://github.com/serilog/serilog) logging framework.

> This library is still experiemental

## Getting Started
Install it from Nuget:
```
Install-Package Nancy.Serilog
```
Enable logging from your Bootstrapper at `ApplicationStartup`, this block is a good place to configure actual logger. 
```cs
using Nancy.Serilog;
// ...
class CustomBootstrapper : DefaultNancyBootstrapper
{
    protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
    {
        pipelines.EnableSerilog();

        Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Information()
           .WriteTo.Console(new JsonFormatter())
           .CreateLogger()
    }
}
```
Now data from your requests will be logged when receiving requests and when returing responses:
```cs
public class Index : NancyModule
{
    public Index()
    {
        Get["/"] = args => "Hello Serilog";
    }
}
```
Without doing any extra configuration, navigating to `/` (root) will be logged: In the following screenshot I am using a self-hosted Nancy app (the sample of this repo) with some ignored fields.

![console](https://user-images.githubusercontent.com/13316248/33915081-af7128e2-dfa1-11e7-8d58-1dd6b191e86a.png)


## Log Correlation
 Notice how the two logs are correlated using `RequestId` property. This is how you find logs coming from a single request. To enable this correlation, you _cannot_ use the globally shared `ILogger` (i.e. from Log.*Something*) because you want to use a logger bound to the request context: Nancy.Serilog provides an extension method `CreateLogger()` you can call from your `NancyModule` like this:

```cs
Post["/hello/{user}"] = args =>
{
    var logger = this.CreateLogger();
    var user = (string)args.user;
    logger.Information("{User} Logged In", user);
    return $"Hello {user}";
};
```

Then `POST`ing some data from Postman will give us the following: 

![post](https://user-images.githubusercontent.com/13316248/33915879-287f96ac-dfa6-11e7-9d59-d176909f9a1f.png)

## Ignoring Fields
Nancy.Serilog will try to retrieve all the information it can get from requests, responses and errors. However, you can still tell the library what fields to ignore from the logs, it goes like this: 
```cs
    class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.EnableSerilog(new Options
            {
                IgnoredResponseLogFields = new string[]
                {
                    "RawResponseCookies",
                    "ResponseHeaders",
                    "ResponseCookies",
                },

                IgnoredRequestLogFields = new string[]
                {
                    "RequestHeaders"
                }
            });


            StaticConfiguration.DisableErrorTraces = false;
        }
    }
```
