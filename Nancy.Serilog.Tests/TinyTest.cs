using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;

namespace TinyTest
{
    public enum TestResult
    {
        Succeeded, Failed, Errored
    }

    public class TestDefinition
    {
        public string Name { get; set; }
        public TestResult Result { get; set; }
        public long RunTime { get; set; }
        public Exception Exception { get; set; } = null;
    }

    public class TestModule
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public List<TestDefinition> Tests { get; set; }
    }

    public interface ITestReporter
    {
        int Report(IEnumerable<TestModule> modules);
    }


    public class ConsoleReporter : ITestReporter
    {
        private int testResult = 0;
        private void LogTests(IEnumerable<TestDefinition> tests, bool indent = false)
        {
            foreach (var test in tests)
            {
                var indentation = indent ? "    " : "";
                if (test.Result == TestResult.Succeeded)
                {
                    SuccessLog(test.Name, test.RunTime);
                }

                if (test.Result == TestResult.Errored)
                {
                    testResult = 1;
                    var ex = test.Exception;
                    ErrorLog(test.Name, test.RunTime);
                    WriteLine($"{ex.GetType().Name}: {ex.Message}");
                    WriteLine(ex.StackTrace);
                }

                if (test.Result == TestResult.Failed)
                {
                    testResult = 1;
                    FailLog(test.Name, test.RunTime);
                }
            }
        }

        private static void ErrorLog(string msg, long runtime)
        {
            Write($"   | --- {msg}");
            Write($" | ");
            ForegroundColor = ConsoleColor.Red;
            Write("Errored");
            ResetColor();
            Write(" in ");
            ForegroundColor = ConsoleColor.Cyan;
            Write(runtime);
            ResetColor();
            Write(" ms");
            Write(Environment.NewLine);
        }

        private static void SuccessLog(string msg, long runtime)
        {
            Write($"   | --- {msg}");
            Write($" | ");
            ForegroundColor = ConsoleColor.Green;
            Write("Passed");
            ResetColor();
            Write(" in ");
            ForegroundColor = ConsoleColor.Cyan;
            Write(runtime);
            ResetColor();
            Write(" ms");
            Write(Environment.NewLine);
        }

        private static void FailLog(string msg, long runtime)
        {
            Write($"   | --- {msg}");
            Write($" | ");
            ForegroundColor = ConsoleColor.Yellow;
            Write("Failed");
            ResetColor();
            Write(" in ");
            ForegroundColor = ConsoleColor.Cyan;
            Write(runtime);
            ResetColor();
            Write(" ms");
            Write(Environment.NewLine);
        }

        public int Report(IEnumerable<TestModule> modules)
        {
            Console.InputEncoding = System.Text.Encoding.Unicode;
            var defaultOnly = modules.Count() == 1 && modules.First().IsDefault;

            if (defaultOnly)
            {
                var defaultModule = modules.First();
                WriteLine($" Module: Default");
                WriteLine("   |");
                LogTests(defaultModule.Tests);
                WriteLine();
                return testResult;
            }

            foreach (var module in modules)
            {
                WriteLine($" Module: {module.Name}");
                WriteLine("   |");
                LogTests(module.Tests, true);
                WriteLine();
            }


            return testResult;
        }
    }

    public static class Test
    {
        class TestFailureException : Exception
        {
            public TestFailureException() { }
            public TestFailureException(string msg) : base(msg)
            {

            }
        }

        public static void Module(string name)
        {
            if (testModules.Count == 1 && testModules[0].IsDefault)
            {
                testModules[0] = new TestModule
                {
                    Name = name,
                    Tests = testModules[0].Tests,
                    IsDefault = false
                };
            }
            else
            {
                testModules.Add(new TestModule
                {
                    Name = name,
                    Tests = new List<TestDefinition>(),
                    IsDefault = false
                });

            }
        }




        static List<TestModule> testModules = new List<TestModule>()
        {
            new TestModule
            {
                Name = "Default",
                IsDefault = true,
                Tests = new List<TestDefinition>()
            }
        };

        private static Action<string, Exception> onError = (name, ex) =>
        {

        };

        public static int ReportUsing(ITestReporter reporter) => reporter.Report(testModules);

        public static int Report() => ReportUsing(new ConsoleReporter());

        public static void Fail(string msg = "")
        {
            if (!string.IsNullOrEmpty(msg))
            {
                throw new TestFailureException(msg);
            }

            throw new TestFailureException();
        }

        public static void Equal<T>(T x, T y) where T : IEquatable<T>
        {
            if (!x.Equals(y))
            {
                Fail();
            }
        }

        public static void ArraysEqual<T>(IEnumerable<T> xs, IEnumerable<T> ys) where T : IEquatable<T>
        {
            if (xs == null && ys == null) { return; } // both null is OK

            var x = xs.ToArray();
            var y = ys.ToArray();

            if (x.Length != y.Length)
            {
                Fail();
            }

            for (var i = 0; i < x.Length; i++)
            {
                if (!x[i].Equals(y[i]))
                {
                    Fail();
                }
            }
        }

        public static void OnError(Action<string, Exception> handler)
        {
            if (handler != null)
            {
                onError = handler;
            }
        }

        public static void Case(string name, Action handler)
        {
            var testResult = new TestDefinition();
            testResult.Name = name;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                handler();
            }
            catch (TestFailureException)
            {
                testResult.Result = TestResult.Failed;
            }
            catch (Exception ex)
            {
                testResult.Result = TestResult.Errored;
                testResult.Exception = ex;
                onError(name, ex);
            }
            finally
            {
                stopwatch.Stop();
                testResult.RunTime = stopwatch.ElapsedMilliseconds;
            }

            var lastModule = testModules.Count - 1;
            testModules[lastModule].Tests.Add(testResult);
        }

        public static Task CaseAsync(string name, Func<Task> handler)
        {
            var testResult = new TestDefinition();
            testResult.Name = name;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var task = handler();
                task.Wait();
                testResult.Result = TestResult.Succeeded;
            }
            catch (TestFailureException)
            {
                testResult.Result = TestResult.Failed;
            }
            catch (Exception ex)
            {
                testResult.Result = TestResult.Errored;
                testResult.Exception = ex;
                onError(name, ex);
            }
            finally
            {
                stopwatch.Stop();
                testResult.RunTime = stopwatch.ElapsedMilliseconds;
            }

            var lastModule = testModules.Count - 1;
            testModules[lastModule].Tests.Add(testResult);

            return Task.Run(() => { });
        }
    }
}
