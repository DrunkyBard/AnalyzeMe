using AnalyzeMe.Design.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AnalyzeMe.Tests
{
    public sealed class CallVirtualInConstructorAnalyzerTests : CodeFixVerifier
    {
        string src = @"
    public class Base
    {
        public virtual void A()
        {
        }
    }

    public class Derived : Base
    {
        public Derived()
        {
            A();
        }

        public override void A()
        {
        }
    }
";

        [Fact]
        public void Test()
        {
            var expected = new DiagnosticResult
            {
                Id = CallVirtualInConstructorAnalyzer.VirtualMethodCallInConstructorId,
                Message = "",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 4, 11)
                        }
            };
            VerifyCSharpDiagnostic(src, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CallVirtualInConstructorAnalyzer();
        }
    }
}
