using System.Collections.Generic;

namespace AnalyzeMe.Tests.TestFixtures
{
    public static class RxSubscribeMethodTestFixtures
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
        public void OnNextHandler(object val){}

        public void OnCompletedHandler(){}

        public void Bar()
        {
            IObservable<object> observable = null;
            {0}
        }
    }
}";

        public static IEnumerable<object[]> OnErrorParameterExists()
        {
            yield return new object[]
            {
                FormatSrc(@"observable.Subscribe(onError: _ => { }, onNext: _ => { }, onCompleted: () => { });")
            };

            yield return new object[]
            {
                FormatSrc(@"observable.Subscribe(nextValue => { }, ex => { }, () => { });")
            };

            yield return new object[]
            {
                FormatSrc(@"observable.Subscribe(nextValue => { }, ex => { });")
            };
        }

        public static IEnumerable<object[]> MethodInvocationDoesNotHaveOnErrorParameter()
        {
            var actual = @"observable.Subscribe(OnNextHandler /*Comment*/);";
            var expected = @"observable.Subscribe(OnNextHandler /*Comment*/, ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                OnNextHandler /*Comment*/);";
            expected = @"observable.Subscribe(
                OnNextHandler /*Comment*/, 
                ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                OnNextHandler /*Comment*/
                );";
            expected = @"observable.Subscribe(
                OnNextHandler /*Comment*/, 
                ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                onNext: OnNextHandler /*Comment*/);";
            expected = @"observable.Subscribe(
                onNext: OnNextHandler /*Comment*/,
                onError: ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                onNext: OnNextHandler /*Comment*/
                );";
            expected = @"observable.Subscribe(
                onNext: OnNextHandler /*Comment*/,
                onError: ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(OnNextHandler /*Comment*/, /*Comment*/OnCompletedHandler);";
            expected = @"observable.Subscribe(OnNextHandler /*Comment*/, ex => { /*TODO: handle this!*/ }, /*Comment*/OnCompletedHandler);";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                OnNextHandler /*Comment*/, /*Comment*/OnCompletedHandler);";
            expected = @"observable.Subscribe(
                OnNextHandler /*Comment*/, ex => { /*TODO: handle this!*/ }, /*Comment*/OnCompletedHandler);";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                OnNextHandler /*Comment*/, 
                /*Comment*/OnCompletedHandler);";
            expected = @"observable.Subscribe(
                OnNextHandler /*Comment*/, 
                           ex => { /*TODO: handle this!*/ }, 
                /*Comment*/OnCompletedHandler);";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                OnNextHandler /*Comment*/,
                /*Comment*/OnCompletedHandler
                );";
            expected = @"observable.Subscribe(
                OnNextHandler /*Comment*/, 
                           ex => { /*TODO: handle this!*/ }, 
                /*Comment*/OnCompletedHandler
                );";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                onNext: OnNextHandler /*Comment*/, onCompleted: /*Comment*/OnCompletedHandler);";
            expected = @"observable.Subscribe(
                onNext: OnNextHandler /*Comment*/, onCompleted: /*Comment*/OnCompletedHandler, onError: ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                onNext: OnNextHandler /*Comment*/,
                onCompleted: /*Comment*/OnCompletedHandler
                );";
            expected = @"observable.Subscribe(
                onNext: OnNextHandler /*Comment*/,
                onCompleted: /*Comment*/OnCompletedHandler,
                onError: ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                _ =>
                {

                } /*
                */);";
            expected = @"observable.Subscribe(
                _ =>
                {

                } /*
                */, 
                ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                _ =>
                {

                } /*Comment after onNext*/
                , () => { });";
            expected = @"observable.Subscribe(
                _ =>
                {

                } /*Comment after onNext*/, 
                ex => { /*TODO: handle this!*/ }, 
                () => { });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                        _ => { }
                        );";
            expected = @"observable.Subscribe(
                        _ => { }, 
                        ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(_ =>
            {

            });";
            expected = @"observable.Subscribe(_ =>
            {

            }, 
            ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe( /* Comment before onNext */
                            nextValue => { }
            );";
            expected = @"observable.Subscribe( /* Comment before onNext */
                            nextValue => { }, 
                            ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe( /* Comment before onNext */
                            nextValue => { }, () => { }
            );";
            expected = @"observable.Subscribe( /* Comment before onNext */
                            nextValue => { }, ex => { /*TODO: handle this!*/ }, () => { }
            );";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                            nextValue => { } /*Trailing onNext comment*/, () => { }
            );";
            expected = @"observable.Subscribe(
                            nextValue => { } /*Trailing onNext comment*/, ex => { /*TODO: handle this!*/ }, () => { }
            );";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                            nextValue => { }, /*Trailing onCompleted comma comment*/ () => { }
            );";
            expected = @"observable.Subscribe(
                            nextValue => { }, ex => { /*TODO: handle this!*/ }, /*Trailing onCompleted comma comment*/ () => { }
            );";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe( /* Comment before onNext */
                            nextValue => { });";
            expected = @"observable.Subscribe( /* Comment before onNext */
                            nextValue => { }, 
                            ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(nextValue => { Console.WriteLine(); },
                () => { /*Some comment*/ });";
            expected = @"observable.Subscribe(nextValue => { Console.WriteLine(); }, 
                ex => { /*TODO: handle this!*/ }, 
                () => { /*Some comment*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected))
            };

            actual = @"observable.Subscribe(
                            nextValue => { Console.WriteLine(); },
                            () => { /*Some comment*/ });";
            expected = @"observable.Subscribe(
                            nextValue => { Console.WriteLine(); }, 
                            ex => { /*TODO: handle this!*/ }, 
                            () => { /*Some comment*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe( /* Comment before onNext */
                            onNext: nextValue => { } /*OnNext argument trailing comment*/
            );";
            expected = @"observable.Subscribe( /* Comment before onNext */
                            onNext: nextValue => { } /*OnNext argument trailing comment*/,
                            onError: ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe( /* Comment before onNext */
                            onNext: nextValue => { } /*OnNext argument trailing comment*/);";
            expected = @"observable.Subscribe( /* Comment before onNext */
                            onNext: nextValue => { } /*OnNext argument trailing comment*/,
                            onError: ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(onCompleted: () => { },
                                 onNext: nextValue => { } /*Trailing onNext comment*/);";
            expected = @"observable.Subscribe(onCompleted: () => { },
                                 onNext: nextValue => { } /*Trailing onNext comment*/,
                                 onError: ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                                    onCompleted: () => { }, onNext: nextValue => { }
            );";
            expected = @"observable.Subscribe(
                                    onCompleted: () => { }, onNext: nextValue => { }, onError: ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                                    onCompleted: () => { }, onNext: nextValue => { });";
            expected = @"observable.Subscribe(
                                    onCompleted: () => { }, onNext: nextValue => { }, onError: ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };

            actual = @"observable.Subscribe(
                                onCompleted: () => {
                                    Console.WriteLine();
                                },
    /* Comment before onNext*/  onNext: nextValue => {
                                    Console.WriteLine();
                                } /*Trailing onNext comment*/);";
            expected = @"observable.Subscribe(
                                onCompleted: () => {
                                    Console.WriteLine();
                                },
    /* Comment before onNext*/  onNext: nextValue => {
                                    Console.WriteLine();
                                } /*Trailing onNext comment*/,
                                onError: ex => { /*TODO: handle this!*/ });";

            yield return new object[]
            {
                new SourceFixture(FormatSrc(actual), FormatSrc(expected)) 
            };
        }

        private static string FormatSrc(string insertionSrc)
        {
            return Source.Replace("{0}", insertionSrc);
        }
    }
}
