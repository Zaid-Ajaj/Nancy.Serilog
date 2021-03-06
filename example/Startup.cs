using Microsoft.AspNetCore.Builder;
using Nancy.Owin;

namespace Example
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(pipeline => 
            {
                pipeline.UseNancy(new NancyOptions 
                {
                    Bootstrapper = new Bootstrapper()
                });
            }); 
        }
    }
}