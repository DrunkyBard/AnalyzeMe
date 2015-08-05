using AnalyzeMe.Design.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace AnalyzeMe.Tests
{
    [TestClass]
    public sealed class RxSubscribeMethodAnalyzerTests : CodeFixVerifier
    {
        private const string Source =
@"using System;

namespace System
{
    public static class ObservableExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source)
        { return null; }

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
            IObservable<object> observable = null;
            {0}
        }
    }
}";

        [TestMethod]
        public void WhenOnErrorParameterExists_ThenExpectNoDiagnostic()
        {
            var source = Source.Replace(@"{0}",
            @"observable.Subscribe(onError: _ => { }, onNext: _ => { }, onCompleted: () => { });
            observable.Subscribe(nextValue => { }, ex => { }, () => { });
            observable.Subscribe(nextValue => { }, ex => { });");

            VerifyCSharpDiagnostic(source);
        }

        [TestMethod]
        public void WhenSubscribeMethodInvocationDoesNotHaveOnErrorParameter_ThenDiagnosticThrown()
        {
            var originSource = Source.Replace(@"{0}",
            @"
            //observable.Subscribe(_ => {});
            observable.Subscribe(
                nextValue => {Console.WriteLine(string.Format(""{0}"", ""acb""))}, 
                () => {});
            observable.Subscribe(nextValue => {});
            observable.Subscribe(onCompleted: () => {}, onNext: nextValue => {});
            ");

            var expectedSource = Source.Replace(@"{0}",
            @"observable.Subscribe(nextValue => {}, ex => { /*TODO: handle this!*/ }, () => {});
            observable.Subscribe(nextValue => {}, ex => { /*TODO: handle this!*/ });
            observable.Subscribe(onCompleted: () => {}, onNext: nextValue => {}, onError: ex => { /*TODO: handle this!*/ });
            ");

            var firstSubscribeMethodInvocationDiagnostic = CreateDiagnostic(28, 13);
            var secondSubscribeMethodInvocationDiagnostic = CreateDiagnostic(29, 13);
            var thirdSubscribeMethodInvocationDiagnostic = CreateDiagnostic(30, 13);

            VerifyCSharpFix(originSource, expectedSource);
            VerifyCSharpDiagnostic(originSource, firstSubscribeMethodInvocationDiagnostic, secondSubscribeMethodInvocationDiagnostic, thirdSubscribeMethodInvocationDiagnostic);
        }

        private DiagnosticResult CreateDiagnostic(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = RxSubscribeMethodAnalyzer.RxSubscribeMethodDiagnosticId,
                Message = RxSubscribeMethodAnalyzer.RxSubscribeMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                     new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column),
                         }
            };
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RxSubscribeMethodAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new RxSubscribeMethodCodeFixProvider();
        }
    }
}
