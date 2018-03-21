using Nancy;

namespace CoreSample
{
    public class Home : NancyModule
    {
        public Home()
        {
            Get("/", args => "Hello From Home");
            Get("/other", args => "Other Route");
        }
    }
}