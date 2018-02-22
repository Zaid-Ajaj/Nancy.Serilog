using System;
using System.IO;
using Nancy;
using Nancy.Serilog;
using System.Threading.Tasks;
using System.Threading;
using Nancy.Cookies;
using System.Linq;
using Nancy.Responses;
using Serilog;

namespace SelfhostedSample
{
    public class FilesModule : NancyModule
    {
        public FilesModule() : base("/files")
        {
            Before += ctx => 
                !ctx.Request.Files.Any() 
                    ? new TextResponse(HttpStatusCode.BadRequest, "Not Found") 
                    : null;

            Get["/"] = args =>
            {
                return "Cannot reach me :( ";
            };
            
            Post["/sync"] = x =>
            {
                var file = Request.Files.FirstOrDefault();
                
                try
                {
                    using (var reader = new StreamReader(file.Value))
                    {
                        var fileContent = reader.ReadToEnd();
                        return Response.AsText($"File Contents: \n{fileContent}");
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error: ");
                    throw;
                }
                
                return Response.AsText("ok");
            };
        }
    }
    
    
    public class HomeModule : NancyModule
    {
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
}