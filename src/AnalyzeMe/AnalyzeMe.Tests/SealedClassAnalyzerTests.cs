using System;
using AnalyzeMe.Design.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AnalyzeMe.Tests
{
    public sealed class SealedClassAnalyzerTests : CodeFixVerifier
    {
        private const string Template = @"
namespace TestNamespace
{
    {classModifier}class TestClass
    {
        {methodModifier}void Foo()
        {methodBody}

        public int I = default(int);

        public {propertyModifier}int J { get; set; }
    }

    sealed class DerivedClass : {baseClass}
    {
    }
}";
        private const string ExpectedSealedClass = @"
namespace TestNamespace
{
    sealed class TestClass
    {
        void Foo()
        { }

        public int I = default(int);

        public int J { get; set; }
    }

    sealed class DerivedClass : object
    {
    }
}";

        [Fact]
        public void ExpectNoDiagnostics()
        {
            var sealedClassDiagnosticCode = ApplyFormat(classModifier: "sealed");
            var staticClassDiagnosticCode = ApplyFormat(classModifier: "static", methodModifier: "static", propertyModifier: "static");
            var abstractClassDiagnosticCode = ApplyFormat(classModifier: "abstract", methodModifier: "abstract");
            var classWithVirtualMethodAndPropertyDiagnosticCode = ApplyFormat(methodModifier: "virtual", propertyModifier: "virtual");
            var withDerivedTypeDiagnosticCode = ApplyFormat(isBase: true);

            VerifyCSharpDiagnostic(sealedClassDiagnosticCode);
            VerifyCSharpDiagnostic(staticClassDiagnosticCode);
            VerifyCSharpDiagnostic(abstractClassDiagnosticCode);
            VerifyCSharpDiagnostic(classWithVirtualMethodAndPropertyDiagnosticCode);
            VerifyCSharpDiagnostic(withDerivedTypeDiagnosticCode);
        }

        [Fact]
        public void ExpectSealedDiagnostic()
        {
            var source = ApplyFormat();
            var expected = new DiagnosticResult
            {
                Id = SealedClassAnalyzer.ClassCanBeSealedDiagnosticId,
                Message = SealedClassAnalyzer.ClassCanBeSealedMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 4, 11)
                        }
            };

            VerifyCSharpDiagnostic(source, expected);
            VerifyCSharpFix(source, ExpectedSealedClass);
        }

        private string ApplyFormat(string classModifier = "", string methodModifier = "", string propertyModifier = "", bool isBase = false)
        {
            var methodBody = methodModifier == "abstract" ? ";" : "{ }";
            Func<string, string> withTrailingTrivia =
                modifier => string.IsNullOrEmpty(modifier) ? String.Empty : $"{modifier} ";

            return Template
                .Replace("{classModifier}", withTrailingTrivia(classModifier))
                .Replace("{methodModifier}", withTrailingTrivia(methodModifier))
                .Replace("{methodBody}", withTrailingTrivia(methodBody))
                .Replace("{propertyModifier}", withTrailingTrivia(propertyModifier))
                .Replace("{baseClass}", isBase ? "TestClass" : "object");
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new SealedClassCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SealedClassAnalyzer();
        }
    }
}