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
            observable.Subscribe( /* Comment before onNext */
                            nextValue => {}
            );
            observable.Subscribe( /* Comment before onNext */
                            nextValue => {}, () => {}
            );
            observable.Subscribe( /* Comment before onNext */
                            nextValue => {});
            observable.Subscribe(nextValue => { Console.WriteLine(); },
                () => { /*Some comment*/ });
            observable.Subscribe(
                            nextValue => { Console.WriteLine(); },
                            () => { /*Some comment*/ });
            observable.Subscribe( /* Comment before onNext */
                            onNext: nextValue => {} /*OnNext argument trailing comment*/
            );
            observable.Subscribe( /* Comment before onNext */
                            onNext: nextValue => {} /*OnNext argument trailing comment*/);
            observable.Subscribe(onCompleted: () => {},
                                 onNext: nextValue => {});
            observable.Subscribe(
                                    onCompleted: () => {}, onNext: nextValue => {}
            );
            observable.Subscribe(
                                    onCompleted: () => {}, onNext: nextValue => {});
            observable.Subscribe(
                                onCompleted: () => {
                                   Console.WriteLine();
                                },
    /* Comment before onNext*/  onNext: nextValue => { 
                                    Console.WriteLine(); 
                                });
            ");

            var expectedSource = Source.Replace(@"{0}",
                @"
            observable.Subscribe( /* Comment before onNext */
                            nextValue => {},
                            ex => { /*TODO: handle this!*/ }
            );
            observable.Subscribe( /* Comment before onNext */
                            nextValue => {}, ex => { /*TODO: handle this!*/ }, () => {}
            );
            observable.Subscribe( /* Comment before onNext */
                            nextValue => {}, ex => { /*TODO: handle this!*/ });
            observable.Subscribe(nextValue => { Console.WriteLine(); },
                ex => { /*TODO: handle this!*/ },
                () => { /*Some comment*/ });
            observable.Subscribe(
                            nextValue => { Console.WriteLine(); },
                            ex => { /*TODO: handle this!*/ },
                            () => { /*Some comment*/ });
            observable.Subscribe( /* Comment before onNext */
                            onNext: nextValue => {}, /*OnNext argument trailing comment*/
                            onError: ex => { /*TODO: handle this!*/ }
            );
            observable.Subscribe( /* Comment before onNext */
                            onNext: nextValue => {}, /*OnNext argument trailing comment*/
                            onError: ex => { /*TODO: handle this!*/ });
            observable.Subscribe(onCompleted: () => {},
                                 onNext: nextValue => {},
                                 onError: ex => { /*TODO: handle this!*/ });
            observable.Subscribe(
                                    onCompleted: () => {}, onNext: nextValue => {}, onError: ex => { /*TODO: handle this!*/ }
            );
            observable.Subscribe(
                                    onCompleted: () => {}, onNext: nextValue => {}, onError: ex => { /*TODO: handle this!*/ });
            observable.Subscribe(
                                onCompleted: () => {
                                   Console.WriteLine();
                                },
    /* Comment before onNext*/  onNext: nextValue => { 
                                    Console.WriteLine(); 
                                },
                                onError: ex => { /*TODO: handle this!*/ });
            ");

            VerifyCSharpDiagnostic(
                originSource,
                CreateDiagnostic(32, 13),
                CreateDiagnostic(35, 13),
                CreateDiagnostic(38, 13),
                CreateDiagnostic(40, 13),
                CreateDiagnostic(42, 13),
                CreateDiagnostic(45, 13),
                CreateDiagnostic(48, 13),
                CreateDiagnostic(50, 13),
                CreateDiagnostic(52, 13),
                CreateDiagnostic(55, 13),
                CreateDiagnostic(57, 13));
            VerifyCSharpFix(originSource, expectedSource);
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
