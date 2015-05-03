using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RoslynAnalyzers.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        private const string Template = @"
using RoslynAnalyzers.TechnicalDebt;

namespace Regex.Console
{
    [TechnicalDebt({0}, {1}, {2}, {3})]
    class Test
    {
        static void Action()
        {
        }
    }
}";

        [TestMethod]
        public void ExpectNoDiagnostics()
        {
            var nextDay = DateTime.Now.AddDays(1);
            var noDiagnosticsCode = ApplyFormat(nextDay.Year, nextDay.Month, nextDay.Day, "Valid reason");

            VerifyCSharpDiagnostic(noDiagnosticsCode);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void ExpectWrongDateDiagnostic()
        {
            var diagnosticCode = ApplyFormat(-1, int.MaxValue, int.MinValue, "Valid reason");
            string errorMessage = null;

            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new DateTime(-1, int.MaxValue, int.MinValue);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                errorMessage = ex.Message;
            }

            var expected = new DiagnosticResult
            {
                Id = TechnicalDebtAnalyzer.DiagnosticId,
                Message = $"Attribute usage error: {errorMessage}",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 6)
                        }
            };
            
            VerifyCSharpDiagnostic(diagnosticCode, expected);
        }

        [TestMethod]
        public void ExpectWrongReasonDiagnostic()
        {
            var nextDay = DateTime.Now.AddDays(1);
            var nullReasonDiagnosticCode = ApplyFormat(nextDay.Year, nextDay.Month, nextDay.Day, null);
            var emptyReasonDiagnosticCode = ApplyFormat(nextDay.Year, nextDay.Month, nextDay.Day, null);

            var expected = new DiagnosticResult
            {
                Id = TechnicalDebtAnalyzer.DiagnosticId,
                Message = "Attribute usage error: Reason parameter should not be null or empty.",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 32)
                        }
            };
            
            VerifyCSharpDiagnostic(nullReasonDiagnosticCode, expected);
            VerifyCSharpDiagnostic(emptyReasonDiagnosticCode, expected);
        }

        [TestMethod]
        public void ExpectExpiredTechnicalDebtDiagnostic()
        {
            var nextDay = DateTime.Now.AddDays(-1);
            var reason = "Valid reason";
            var diagnosticCode = ApplyFormat(nextDay.Year, nextDay.Month, nextDay.Day, reason);

            var expected = new DiagnosticResult
            {
                Id = TechnicalDebtAnalyzer.DiagnosticId,
                Message = $"Technical debt with reason \'{reason}\' already expired.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 6)
                        }
            };
            
            VerifyCSharpDiagnostic(diagnosticCode, expected);
        }

        private string ApplyFormat(int year, int month, int day, string reason)
        {
            reason = reason == null ? "null" : $@"""{reason}""";

            return Template
                .Replace("{0}", year.ToString())
                .Replace("{1}", month.ToString())
                .Replace("{2}", day.ToString())
                .Replace("{3}", reason);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new RoslynAnalyzersCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TechnicalDebtAnalyzer();
        }
    }
}
