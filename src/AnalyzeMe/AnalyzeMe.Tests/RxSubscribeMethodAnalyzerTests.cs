using AnalyzeMe.Design.Analyzers;
using AnalyzeMe.WorkProcess.Analyzers;
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

namespace Test
{
    public class Foo
    {
        public void Bar()
        {
            Console.WriteLine(1);
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
