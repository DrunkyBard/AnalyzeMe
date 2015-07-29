using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzeMe.Design.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RxSubscribeMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string RxSubscribeMethodDiagnosticId = "RxSubscribeMethodUsage";
        internal static readonly LocalizableString RxSubscribeMethodTitle = "Placeholder";
        internal static readonly LocalizableString RxSubscribeMessageFormat = "Placeholder";
        internal const string RxSubscribeMethodCategory = "Usage";
        internal static readonly DiagnosticDescriptor RxSubscribeMethodRule = new DiagnosticDescriptor(
            RxSubscribeMethodDiagnosticId,
            RxSubscribeMethodTitle,
            RxSubscribeMessageFormat,
            RxSubscribeMethodCategory,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RxSubscribeMethodRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Action1, SyntaxKind.InvocationExpression);
            //context.RegisterSymbolAction(Action, SymbolKind.Method);
        }

        private void Action1(SyntaxNodeAnalysisContext ctx)
        {
            var methodInvokationSymbol = ctx.SemanticModel.GetSymbolInfo(ctx.Node);
            var n = ctx.Node;
        }

        private void Action(SymbolAnalysisContext ctx)
        {
            var methodCallSymbol = ctx.Symbol;
        }
    }
}
