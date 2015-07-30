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
            context.RegisterSyntaxNodeAction(AnalyzeMethodInvocation, SyntaxKind.InvocationExpression);
            //context.RegisterSymbolAction(Action, SymbolKind.Method);
        }

        private void AnalyzeMethodInvocation(SyntaxNodeAnalysisContext ctx)
        {
            var methodInvokationSymbol = (IMethodSymbol)ctx.SemanticModel.GetSymbolInfo(ctx.Node).Symbol;
            const string observableExtensionsTypeName = "System.ObservableExtensions";
            const string subscribeMethodName = "Subscribe";

            if (
                methodInvokationSymbol.Name != subscribeMethodName ||
                !methodInvokationSymbol.IsExtensionMethod || 
                !methodInvokationSymbol.IsGenericMethod ||
                methodInvokationSymbol.ContainingType?.ToDisplayString() != observableExtensionsTypeName
                )
            {
                return;
            }
        }
    }
}
