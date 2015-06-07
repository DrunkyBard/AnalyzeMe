using System;
using AnalyzeMe.WorkProcess.Analyzers;
using AnalyzeMe.WorkProcess.Tools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace AnalyzeMe.Tests
{
    [TestClass]
    public class TechnicalDebtAnalyzerTests : CodeFixVerifier
    {
        private const string Template = @"
using AnalyzeMe.WorkProcess.Tools;

namespace TestNamespace
{
    [TechnicalDebt({0}, {1}, {2}, {3})]
    class TestClass
    {
        static void Action()
        {
        }
    }
}";

        [TestMethod]
        public void ExpectWrongDateDiagnostic()
        {
            var diagnosticCode = ApplyFormat(-1, Month.January, int.MinValue, "Valid reason");
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
                Id = TechnicalDebtAnalyzer.AttributeUsageDiagnosticId,
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
            var month = (Month) nextDay.Month;
            var nullReasonDiagnosticCode = ApplyFormat(nextDay.Year, month, nextDay.Day, null);
            var emptyReasonDiagnosticCode = ApplyFormat(nextDay.Year, month, nextDay.Day, null);

            var expected = new DiagnosticResult
            {
                Id = TechnicalDebtAnalyzer.AttributeUsageDiagnosticId,
                Message = "Attribute usage error: Reason parameter should not be null or empty.",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 69)
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
            var diagnosticCode = ApplyFormat(nextDay.Year, (Month)nextDay.Month, nextDay.Day, reason);

            var expected = new DiagnosticResult
            {
                Id = TechnicalDebtAnalyzer.TechnicalDebtExpiredDiagnosticId,
                Message = $"Technical debt with reason \'{reason}\' already expired.",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 6)
                        }
            };
            
            VerifyCSharpDiagnostic(diagnosticCode, expected);
        }

        [TestMethod]
        public void ExpectExpiredSoonTechnicalDebtDiagnostic()
        {
            var nextDay = DateTime.Now.AddDays(1).Date;
            var expiredDays = nextDay.Subtract(DateTime.Now.Date).TotalDays;
            var reason = "Valid reason";
            var diagnosticCode = ApplyFormat(nextDay.Year, (Month)nextDay.Month, nextDay.Day, reason);

            var expected = new DiagnosticResult
            {
                Id = TechnicalDebtAnalyzer.TechnicalDebtExpiredSoonDiagnosticId,
                Message = $"Technical debt with reason \'{reason}\' expired after {expiredDays} days.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 6)
                        }
            };
            
            VerifyCSharpDiagnostic(diagnosticCode, expected);
        }

        private string ApplyFormat(int year, Month month, int day, string reason)
        {
            reason = reason == null ? "null" : $@"""{reason}""";
            
            return Template
                .Replace("{0}", year.ToString())
                .Replace("{1}", string.Join(".", typeof(Month).ToString(), month.ToString()))
                .Replace("{2}", day.ToString())
                .Replace("{3}", reason);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TechnicalDebtAnalyzer();
        }
    }
}
