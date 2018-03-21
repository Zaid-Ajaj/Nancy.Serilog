namespace CoreSample
{
    using Microsoft.AspNetCore.Builder;
    using Nancy.Owin;

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(x => x.UseNancy(new NancyOptions 
            {
                Bootstrapper = new Bootstrapper()
            }));
        }
    }
}