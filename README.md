# Nancy.Serilog [![Nuget](https://img.shields.io/nuget/v/Nancy.Serilog.svg?colorB=green)](https://www.nuget.org/packages/Nancy.Serilog)

[Nancy](https://github.com/NancyFx/Nancy) plugin for application-wide logging using the [Serilog](https://github.com/serilog/serilog) logging framework.

## Available Packages:

| Package | Nancy  | Version |
| --------- | ------------- | ------------- |
| Nancy.Serilog | 1.4.x (Classic .NET)  | [![Nuget](https://img.shields.io/nuget/v/Nancy.Serilog.svg?colorB=green)](https://www.nuget.org/packages/Nancy.Serilog) |
| Nancy.Serilog.Core | 2.0-clinteastwood  | [![Nuget](https://img.shields.io/nuget/v/Nancy.Serilog.Core.svg?colorB=green)](https://www.nuget.org/packages/Nancy.Serilog.Core)  |

## Getting Started
Install it from Nuget:
```bash
# classic .NET (4.5+)
Install-Package Nancy.Serilog 
# Dotnet core
dotnet add package Nancy.Serilog.Core
```
Enable logging from your Bootstrapper at `ApplicationStartup`, this block is a good place to configure the actual logger. 
```cs
using Nancy.Serilog;
// ...
class CustomBootstrapper : DefaultNancyBootstrapper
{
    protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
    {
        pipelines.EnableSerilog();

        // Configure logger to output json-formatted logs to the console
        // or use the sink of your choice 
        Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Information()
           .WriteTo.Console(new JsonFormatter())
           .CreateLogger()
    }
    
    protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
    {
        // Register request based dependency
        container.Register((tinyIoc, namedParams) => context.GetContextualLogger());
        
        // other dependencies using ILogger should be registered here as well
        container.Register<IThirdPartyService, ThirdPartyService>(); 
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
 Notice how the two logs are correlated with the same `RequestId` property. This property is attached to requests and responses so that you can find logs coming from a single roundtrip. When you write custom log messages, you want to include this `RequestId` property to your custom logs so that these too will be correlated to the same requests and responses. To do that, you want to use a logger that is *bound* to the request context: Nancy.Serilog provides an extension method called `GetContexualLogger()` you can register as the request container level from your `Bootstrapper` as in the following snippet.

```csharp
public class Users : NancyModule
{
    // have ILogger as a dependency
    public Users(ILogger logger)
    {
        Post["/hello/{user}"] = args =>
        {
            var user = (string)args.user;
            logger.Information("{User} Logged In", user);
            return $"Hello {user}";
        };
    }
}

public class Bootstrapper : DefaultNancyBootstrapper
{
    protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
    {
        pipelines.EnableSerilog();
        StaticConfiguration.DisableErrorTraces = false;
    }
    
    protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
    {
        // Register request based dependency
        container.Register((tinyIoc, namedParams) => context.GetContextualLogger());
        
        // other dependencies using ILogger should be registered here as well
        container.Register<IThirdPartyService, ThirdPartyService>(); 
    }
}
```

Then `POST`ing some data from Postman will give us the following (using [Seq](https://getseq.net/) to browse log data), see how the second log message also has `RequestId`:  

![post](https://user-images.githubusercontent.com/13316248/33915879-287f96ac-dfa6-11e7-9d59-d176909f9a1f.png)

## Ignoring Fields
Nancy.Serilog will try to retrieve all the information it can get from requests, responses and errors. However, you can still tell the library what fields to ignore *fluently* from the logs, it goes like this: 
```cs
    class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.EnableSerilog(new Options
            {
                IgnoredResponseLogFields = 
                    Ignore.FromResponse()
                          .Field(res => res.RawResponseCookies)
                          .Field(res => res.ReponseHeaders)
                          .Field(res => res.ResponseCookies),

                IgnoredRequestLogFields = 
                    Ignore.FromRequest()
                          .Field(req => req.Method)
                          .Field(req => req.RequestHeaders),

                IgnoredErrorLogFields = 
                    Ignore.FromError() 
                          .Field(error => error.ResolvedRouteParameters)
                          .Field(error => error.StatusCode)
            });
        }
    }
```
