using System.Linq;
using AnalyzeMe.Design.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using TestHelper;
using Xunit;

namespace AnalyzeMe.Tests
{
    public class NoStructDefaultConstructorTests : DiagnosticVerifier
    {
        private string _source = @"
using System;
using AnalyzeMe.WorkProcess.Tools;

namespace AnalyzeMe.WorkProcess.Tools
{
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class NoDefaultConstructorAttribute : Attribute
    { }
}

namespace Test
{
    [NoDefaultConstructor]
    public struct S
    {
        public S(int i){
        }

    }

    public class A
    {
        public void T()
        {
            var t = new S();
            t.Q();
            t.Q();
            t.Q();
        }
    }
}

";

        private string _source1 = @"

 public struct A
    {
        public A(int i)
        {
            
        }
    }

    public class Q<T>
    {
    }

    public class W
    {
        public void A<T>() where T : new()
        {
            new T();
        }
    }

    public class Test
    {
        public void A()
        {
            var q = new Q<A>();
            var w = new W();
            w.A<A>();
        }
    }
";
        [Fact]
        public void Test()
        {
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

            VerifyCSharpDiagnostic(_source1, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NoStructDefaultConstructorAnalyzer();
        }
    }
}