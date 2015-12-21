using System;
using AnalyzeMe.Design.Analyzers;
using AnalyzeMe.Tests.TestFixtures;
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
        public Base()
        {
            {0}
        }

        public virtual void A()
        {
        }
    }

    public class Derived : Base
    {
        public Derived()
        {
            {1}
        }

        {2}
    }

    public class AnotherDerived : Derived
    {
        public AnotherDerived()
        {
            {3}
        }{4}
    }
";
        private const string OverrideMethodDeclaration = @"
        public override void A()
        {
        }";

        private const string OverrideSealedMethodDeclaration = @"
        public override sealed void A()
        {
        }";

        private const string VirtualMethodCall = "A();";

        private SourceFixture FormatSrc(
            bool baseCtorContainsVirtualMethodCall,
            bool derivedClassContainsVirtualMethodCall,
            bool derivedClassOverrideVirtualMethod,
            bool derivedSubTypeCtorContainsVirtualMethodCall,
            bool derivedSubTypeOverrideVirtualMethod)
        {
            var baseCtorVirtualMethodCall = baseCtorContainsVirtualMethodCall ? VirtualMethodCall : string.Empty;
            var derivedClassVirtualMethodCall = derivedClassContainsVirtualMethodCall ? VirtualMethodCall : string.Empty;
            var derivedClassVirtualMethodOverride = derivedClassOverrideVirtualMethod ? OverrideMethodDeclaration : string.Empty;
            var derivedSubTypeCtorVirtualMethodCall = derivedSubTypeCtorContainsVirtualMethodCall ? VirtualMethodCall : string.Empty;
            var derivedSubTypeVirtualMethodOverride = derivedSubTypeOverrideVirtualMethod ? Environment.NewLine + Environment.NewLine + OverrideMethodDeclaration : string.Empty;
            var originSrc = src
                .Replace("{0}", baseCtorVirtualMethodCall)
                .Replace("{1}", derivedClassVirtualMethodCall)
                .Replace("{2}", derivedClassVirtualMethodOverride)
                .Replace("{3}", derivedSubTypeCtorVirtualMethodCall)
                .Replace("{4}", derivedSubTypeVirtualMethodOverride);
            var fixedSrc = src
                .Replace("{0}", baseCtorVirtualMethodCall)
                .Replace("{1}", derivedClassVirtualMethodCall)
                .Replace("{2}", derivedClassContainsVirtualMethodCall && derivedClassOverrideVirtualMethod && !derivedSubTypeOverrideVirtualMethod 
                                    ? OverrideSealedMethodDeclaration 
                                    : OverrideMethodDeclaration)
                .Replace("{3}", derivedSubTypeCtorVirtualMethodCall)
                .Replace("{4}", derivedSubTypeOverrideVirtualMethod
                                    ? derivedSubTypeCtorContainsVirtualMethodCall ? OverrideSealedMethodDeclaration : OverrideMethodDeclaration
                                    : string.Empty);
            
            return new SourceFixture(originSrc, fixedSrc);
        }

        [Fact]
        public void
            WhenConstructorContainsVirtualMethodCall_And_DerivedClassOverrideVirtualMethod_And_AllDerivedClassSubTypesDoesNotOverrideVirtualMethod_ThenDiagnosticShouldBeThrown_And_OverridesMethodShouldBeMarkedAsSealed
            ()
        {
            var source = FormatSrc(false, true, true, false, false);
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = CallVirtualInConstructorAnalyzer.MethodCanBeMarkedAsSealedId,
                Message = CallVirtualInConstructorAnalyzer.VirtualMethodCallInConstructorMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 18, 13)
                        }
            };

            VerifyCSharpDiagnostic(source.Actual, expectedDiagnostic);
            VerifyCSharpFix(source.Actual, source.Expected);
        }

        [Fact]
        public void
            WhenConstructorContainsVirtualMethodCall_And_DerivedClassOverrideVirtualMethod_And_AllDerivedClassSubTypesOverrideVirtualMethod_ThenDiagnosticShouldBeThrown_And_SubTypesOverridesMethodShouldBeMarkedAsSealed
            ()
        {
            var source = FormatSrc(false, true, true, true, true);
            var expectedDiagnostic1 = new DiagnosticResult
            {
                Id = CallVirtualInConstructorAnalyzer.VirtualMethodCallInConstructorId,
                Message = CallVirtualInConstructorAnalyzer.VirtualMethodCallInConstructorMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 18, 13)
                        }
            };
            var expectedDiagnostic2 = new DiagnosticResult
            {
                Id = CallVirtualInConstructorAnalyzer.MethodCanBeMarkedAsSealedId,
                Message = CallVirtualInConstructorAnalyzer.VirtualMethodCallInConstructorMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 31, 13)
                        }
            };

            VerifyCSharpDiagnostic(source.Actual, expectedDiagnostic1, expectedDiagnostic2);
            VerifyCSharpFix(source.Actual, source.Expected);
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
