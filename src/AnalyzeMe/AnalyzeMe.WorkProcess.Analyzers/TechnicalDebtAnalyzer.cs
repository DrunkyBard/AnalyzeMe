using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AnalyzeMe.WorkProcess.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace AnalyzeMe.WorkProcess.Analyzers
{
    /// <summary>
    /// Analyze code for correctness of use TechnicalDebtAttribute and check if technical debt already expired.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TechnicalDebtAnalyzer : DiagnosticAnalyzer
    {
        public const string AttributeUsageDiagnosticId = "TechnicalDebtAttributeUsage";
        public const string TechnicalDebtExpiredDiagnosticId = "TechnicalDebtExpired";
        public const string TechnicalDebtExpiredSoonDiagnosticId = "TechnicalDebtExpiredSoon";

        internal static readonly LocalizableString AttributeUsageTitle = "Incorrect attribute usage.";
        internal static readonly LocalizableString AttributeUsageMessageFormat = "Attribute usage error: {0}";
        internal const string AttributeUsageCategory = "Usage";

        internal static readonly LocalizableString DebtExpiredTitle = "Technical debt expired. Redesign this!";
        internal static readonly LocalizableString DebtExpiredMessageFormat = "Technical debt with reason \'{0}\' already expired.";
        internal static readonly string DebtExpiredCategory = "Design";

        internal static readonly LocalizableString DebtExpiredSoonTitle = "Technical debt expired soon. Redesign this!";
        internal static readonly LocalizableString DebtExpiredSoonMessageFormat = "Technical debt with reason \'{0}\' expired after {1} days.";
        internal static readonly string DebtExpiredSoonCategory = "Design";

        internal static readonly DiagnosticDescriptor AttributeUsageRule = new DiagnosticDescriptor(AttributeUsageDiagnosticId, AttributeUsageTitle, AttributeUsageMessageFormat, AttributeUsageCategory, DiagnosticSeverity.Error, isEnabledByDefault: true);
        internal static readonly DiagnosticDescriptor DebtExpiredRule = new DiagnosticDescriptor(TechnicalDebtExpiredDiagnosticId, DebtExpiredTitle, DebtExpiredMessageFormat, DebtExpiredCategory, DiagnosticSeverity.Error, isEnabledByDefault: true);
        internal static readonly DiagnosticDescriptor DebtExpiredSoonRule = new DiagnosticDescriptor(TechnicalDebtExpiredSoonDiagnosticId, DebtExpiredSoonTitle, DebtExpiredSoonMessageFormat, DebtExpiredSoonCategory, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AttributeUsageRule, DebtExpiredRule, DebtExpiredSoonRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private void AnalyzeAttribute(SyntaxNodeAnalysisContext ctx)
        {            
            var attributeSyntax = (AttributeSyntax)ctx.Node;
            var attributeSymbol = ctx.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;
            var arguments = attributeSyntax.ArgumentList;

            if (!attributeSymbol?.ToString().StartsWith("AnalyzeMe.WorkProcess.Tools.TechnicalDebtAttribute") ?? true)
            {
                return;
            }

            if (arguments == null || arguments.Arguments.Count < 4)
            {
                return;
            }

            var yearExpression = arguments.Arguments[0].Expression;
            var monthExpression = arguments.Arguments[1].Expression;
            var dayExpression = arguments.Arguments[2].Expression;
            var reasonExpression = arguments.Arguments[3].Expression;
            var year = ctx.SemanticModel.GetConstantValue(yearExpression);
            var month = ctx.SemanticModel.GetConstantValue(monthExpression);
            var day = ctx.SemanticModel.GetConstantValue(dayExpression);
            var reason = ctx.SemanticModel.GetConstantValue(reasonExpression);

            if (!(year.Is<int>() && month.Is<int>() && day.Is<int>() && (reason.Value == null || reason.Is<string>())))
            {
                return;
            }

            var yearValue = (int)year.Value;
            var monthValue = (int)month.Value;
            var dayValue = (int)day.Value;
            var reasonValue = (string)reason.Value;
            var attributeLocation = attributeSyntax.GetLocation();
            var reasonParameterLocation = reasonExpression.GetLocation();
            var attributeUsageDiagnostics = CheckAttributeUsage(yearValue, monthValue, dayValue, reasonValue, attributeLocation, reasonParameterLocation);

            foreach (var attributeUsageDiagnostic in attributeUsageDiagnostics)
            {
                ctx.ReportDiagnostic(attributeUsageDiagnostic);
            }

            if (attributeUsageDiagnostics.Any())
            {
                return;
            }

            var expiredDebtDiagnostic = CheckIsDebtExpired(new DateTime(yearValue, monthValue, dayValue), reasonValue, attributeLocation);
            ctx.ReportDiagnostic(expiredDebtDiagnostic);
        }

        private IReadOnlyCollection<Diagnostic> CheckAttributeUsage(int year, int month, int day, string reason, Location attributeLocation, Location reasonLocation)
        {
            var diagnostics = new List<Diagnostic>(2);

            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new DateTime(year, month, day); // Just for exception checking
            }
            catch (ArgumentOutOfRangeException ex)
            {
                var diagnostic = Diagnostic.Create(AttributeUsageRule, attributeLocation, ex.Message);
                diagnostics.Add(diagnostic);
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                var emptyReasonDiagnostic = Diagnostic.Create(AttributeUsageRule, reasonLocation, "Reason parameter should not be null or empty.");
                diagnostics.Add(emptyReasonDiagnostic);
            }

            return diagnostics;
        }

        private Diagnostic CheckIsDebtExpired(DateTime expiredDate, string debtReason, Location attributeLocation)
        {
            var nowDate = DateTime.Now.Date;

            if (expiredDate > nowDate)
            {
                return Diagnostic.Create(DebtExpiredSoonRule, attributeLocation, debtReason, expiredDate.Subtract(nowDate).TotalDays);
            }

            return Diagnostic.Create(DebtExpiredRule, attributeLocation, debtReason);
        }
    }
}
