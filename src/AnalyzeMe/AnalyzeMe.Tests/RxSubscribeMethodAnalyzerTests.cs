using AnalyzeMe.Design.Analyzers;
using AnalyzeMe.Tests.TestFixtures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AnalyzeMe.Tests
{    
    public sealed class RxSubscribeMethodAnalyzerTests : CodeFixVerifier
    {
        [Theory, MemberData("OnErrorParameterExists", MemberType = typeof(RxSubscribeMethodTestFixtures))]
        public void WhenOnErrorParameterExists_ThenExpectNoDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source);
        }

        [Theory, MemberData("MethodInvocationDoesNotHaveOnErrorParameter", MemberType = typeof(RxSubscribeMethodTestFixtures))]
        public void WhenSubscribeMethodInvocationDoesNotHaveOnErrorParameter_ThenDiagnosticShouldBeThrown(SourceFixture srcFixture)
        {
            VerifyCSharpDiagnostic(srcFixture.Actual, CreateDiagnostic(35, 13));
            VerifyCSharpFix(srcFixture.Actual, srcFixture.Expected);
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
