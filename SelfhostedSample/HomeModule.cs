using System;
using System.IO;
using Nancy;
using Nancy.Serilog;
using System.Threading.Tasks;
using System.Threading;
using Nancy.Cookies;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Nancy.Responses;
using Serilog;

namespace SelfhostedSample
{
    public interface IThirdParty
    {
        void DoSomething();
    }

    public interface IOtherThirdParty
    {
        void Execute();
    }
    public class OtherThirdParty : IOtherThirdParty
    {
        private readonly ILogger logger;

        public OtherThirdParty(ILogger logger)
        {
            this.logger = logger;
        }
        
        public void Execute()
        {
            logger.Information("Other third party: Execute()");
        }
    }
    
    
    public class ThirdParty : IThirdParty
    {
        private readonly IOtherThirdParty otherThirdParty;
        private readonly ILogger logger; 
        
        public ThirdParty(IOtherThirdParty otherThirdParty, ILogger logger)
        {
            this.logger = logger;
            this.otherThirdParty = otherThirdParty;
        }

        public void DoSomething()
        {
            logger.Information("Third party: DoSomething()");
            otherThirdParty.Execute();
        }
    }
    
    public class HomeModule : NancyModule
    {
        public HomeModule(IThirdParty thirdParty, ILogger logger)
        {
            Get["/"] = args => 
            { 
                thirdParty.DoSomething();
                return $"{Thread.CurrentThread.ManagedThreadId}";
            };

            Post["/hello/{user}"] = args =>
            {
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

            Post["/echo"] = args =>
            {
                var inputStream = Context.Request.Body;
                using (var reader = new StreamReader(inputStream))
                {
                    var content = reader.ReadToEnd();
                    return content;
                }
            };
        }
    }
}