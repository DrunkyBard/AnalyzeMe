using System;
using AnalyzeMe.Design.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace AnalyzeMe.Tests
{
    [TestClass]
    public sealed class RxSubscribeMethodAnalyzerTests : CodeFixVerifier
    {
        private const string Code = @"
using System;

namespace System
{
    public static class ObservableExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
        { return null; }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted)
        { return null; }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError)
        { return null; }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
        { return null; }
    }
}

namespace Test
{
    public class Foo
    {
        public void Bar()
        {
            IObservable<object> obs = null;
            obs.Subscribe<object>(_ => {});
        }
    }
}
";

        [TestMethod]
        public void Test()
        {
            var expected = new DiagnosticResult
            {
                Id = RxSubscribeMethodAnalyzer.RxSubscribeMethodDiagnosticId,
                Message = "Placeholder",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 70)
                        }
            };

            VerifyCSharpDiagnostic(Code, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RxSubscribeMethodAnalyzer();
        }
    }
}
