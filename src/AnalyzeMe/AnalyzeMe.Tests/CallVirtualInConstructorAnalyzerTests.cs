using AnalyzeMe.Design.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
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
        string fixedSrc = @"
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

        public override sealed void A()
        {
        }
    }
";

        [Fact]
        public void Test()
        {
            var expected = new DiagnosticResult
            {
                Id = CallVirtualInConstructorAnalyzer.MethodCanBeMarkedAsSealedId,
                Message = CallVirtualInConstructorAnalyzer.VirtualMethodCallInConstructorMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 13)
                        }
            };

            VerifyCSharpDiagnostic(src, expected);
            VerifyCSharpFix(src, fixedSrc);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CallVirtualInConstructorAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CallVirtualInConstructorCodeFixProvider();
        }
    }
}
