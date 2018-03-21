using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace CoreSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseKestrel()
                .UseUrls("http://localhost:8080")
                .Build();

            host.Run();
        }
    }
}
