using System;
using System.Linq;
using AnalyzeMe.Design.Analyzers;
using AnalyzeMe.Tests.TestFixtures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
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
            //VerifyCSharpDiagnostic(
            //    originSource,
            //    CreateDiagnostic(32, 13),
            //    CreateDiagnostic(38, 13),
            //    CreateDiagnostic(44, 13),
            //    CreateDiagnostic(47, 13),
            //    CreateDiagnostic(51, 13),
            //    CreateDiagnostic(54, 13),
            //    CreateDiagnostic(57, 13),
            //    CreateDiagnostic(60, 13),
            //    CreateDiagnostic(63, 13),
            //    CreateDiagnostic(65, 13),
            //    CreateDiagnostic(67, 13),
            //    CreateDiagnostic(70, 13),
            //    CreateDiagnostic(73, 13),
            //    CreateDiagnostic(75, 13),
            //    CreateDiagnostic(77, 13),
            //    CreateDiagnostic(80, 13),
            //    CreateDiagnostic(82, 13));
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
