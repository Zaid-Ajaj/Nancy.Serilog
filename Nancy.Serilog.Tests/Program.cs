using TinyTest;

namespace Nancy.Serilog.Tests
{
    class Program
    {
        static int Main(string[] args)
        {
            // Using TinyTest
            Test.Module("Fluent Ignore Tests");

            Test.Case("Ignoring request fields works", () =>
            {
                var ignored = Ignore.FromRequest()
                                    .Field(req => req.Method)
                                    .Field(req => req.Path)
                                    .Field(req => req.RequestHeaders)
                                    .ToArray();

                Test.Equal(ignored[0], "Method");
                Test.Equal(ignored[1], "Path");
                Test.Equal(ignored[2], "RequestHeaders");
            });


            Test.Case("Ignoring response fields works", () =>
            {
                var ignored = Ignore.FromResponse()
                                    .Field(res => res.RawResponseCookies)
                                    .Field(req => req.ReasonPhrase)
                                    .ToArray();

                Test.Equal(ignored[0], "RawResponseCookies");
                Test.Equal(ignored[1], "ReasonPhrase");
            });


            Test.Case("Ignoring error fields works", () =>
            {
                var ignored = Ignore.FromError()
                                    .Field(error => error.Duration)
                                    .Field(error => error.ResolvedRouteParameters)
                                    .ToArray();

                Test.Equal(ignored[0], "Duration");
                Test.Equal(ignored[1], "ResolvedRouteParameters");
            });


            Test.Case("FieldChoser<T> returns empty array when no fields had been chosen", () => 
            {
                var defaultChoser = new FieldChooser<ErrorLogData>();

                Test.Equal(0, defaultChoser.ToArray().Length);
            });

            return Test.Report();
        }
    }
}
