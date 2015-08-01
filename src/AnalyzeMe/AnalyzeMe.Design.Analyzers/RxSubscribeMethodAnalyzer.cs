using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzeMe.Design.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RxSubscribeMethodAnalyzer : DiagnosticAnalyzer
    {
        private const string ObservableExtensionsTypeName = "System.ObservableExtensions";
        private const string DisposableTypeName = "System.IDisposable";
        private const string ActionOfTTypeName = "System.Action<T>";
        private const string ExceptionTypeName = "System.Exception";
        private const string SubscribeMethodName = "Subscribe";
        private const string OnErrorParameterName = "onError";

        public const string RxSubscribeMethodDiagnosticId = "RxSubscribeMethodUsage";
        internal static readonly LocalizableString RxSubscribeMethodTitle = @"""Subscribe"" method usage without OnError parameter";
        internal static readonly LocalizableString RxSubscribeMessageFormat = 
            @"Potentially dangerous ""Subscribe"" method invocation: if you use an overload that does not specify a delegate for the OnError notification, any OnError notifications will be re-thrown as an exception.";
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
        }

        private void AnalyzeMethodInvocation(SyntaxNodeAnalysisContext ctx)
        {            
            var methodInvokationSymbol = (IMethodSymbol)ctx.SemanticModel.GetSymbolInfo(ctx.Node).Symbol;
            
            if (
                methodInvokationSymbol.Name != SubscribeMethodName ||
                !methodInvokationSymbol.IsExtensionMethod || 
                !methodInvokationSymbol.IsGenericMethod ||
                methodInvokationSymbol.ReturnType.TypeKind != TypeKind.Interface ||      
                methodInvokationSymbol.ReturnType.ToDisplayString() != DisposableTypeName ||
                methodInvokationSymbol.ContainingType?.ToDisplayString() != ObservableExtensionsTypeName
                )
            {
                return;
            }

            var parameters = methodInvokationSymbol.Parameters;
            var subscribeInvokationContainsOnErrorParameter = parameters
                .Any(p => 
                    p.Name == OnErrorParameterName && 
                    p.Type.OriginalDefinition.ToDisplayString() == ActionOfTTypeName && 
                    ((INamedTypeSymbol)p.Type).TypeArguments.SingleOrDefault(t => t.ToDisplayString() == ExceptionTypeName) != null);

            if (!subscribeInvokationContainsOnErrorParameter)
            {
                var nodeLocation = ctx.Node.GetLocation();
                var diagnostic = Diagnostic.Create(RxSubscribeMethodRule, nodeLocation);
                ctx.ReportDiagnostic(diagnostic);
            }
        }
    }
}
