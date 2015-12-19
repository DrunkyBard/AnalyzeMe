using System;
using System.Collections.Immutable;
using System.Linq;
using AnalyzeMe.Design.Analyzers.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzeMe.Design.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CallVirtualInConstructorAnalyzer : DiagnosticAnalyzer
    {
        public CallVirtualInConstructorAnalyzer()
        {
            
        }

        public const string VirtualMethodCallInConstructorId = "CA2214:DoNotCallOverridableMethodsInConstructors";
        internal static readonly LocalizableString VirtualMethodCallInConstructorTitle = "Class can be marked as sealed.";
        internal static readonly LocalizableString VirtualMethodCallInConstructorMessageFormat = 
            "The constructor of an unsealed type calls a virtual method defined in its class. " +
            "When a virtual method is called, the actual type that executes the method is not selected until run time. " +
            "When a constructor calls a virtual method, it is possible that the constructor for the instance that invokes the method has not executed.";
        internal const string VirtualMethodCallInConstructorCategory = "Usage";
        internal static readonly DiagnosticDescriptor VirtualMethodCallInConstructorRule = new DiagnosticDescriptor(
            VirtualMethodCallInConstructorId,
            VirtualMethodCallInConstructorTitle,
            VirtualMethodCallInConstructorMessageFormat,
            VirtualMethodCallInConstructorCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(VirtualMethodCallInConstructorRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(DetectVirtualCallInConstructor, SyntaxKind.ConstructorDeclaration);
        }

        private void DetectVirtualCallInConstructor(SyntaxNodeAnalysisContext ctx)
        {
            var ctorDeclaration = (ConstructorDeclarationSyntax)ctx.Node;
            var ctorSemantic = ctx.SemanticModel.GetDeclaredSymbol(ctorDeclaration);
            var classType = ctorSemantic.ContainingType;
            var semanticModel = ctx.SemanticModel;

            if (ctorSemantic.IsStatic || 
                classType.IsSealed || 
                classType.IsStatic)
            {
                return;
            }

            var methods = ctorDeclaration
                .Body
                .Statements
                .Where(x => x.IsKind(SyntaxKind.ExpressionStatement))
                .Select(x => ((ExpressionStatementSyntax) x).Expression.As<InvocationExpressionSyntax>())
                .ToArray();

            foreach (var optional in methods)
            {
                if (optional.HasValue && IsMethodVirtual(optional.Value, new Lazy<IMethodSymbol>(() => (IMethodSymbol)semanticModel.GetSymbolInfo(optional.Value).Symbol)))
                {
                    Diagnostic.Create(VirtualMethodCallInConstructorRule, optional.Value.GetLocation());
                }
            }
        }

        private bool IsMethodVirtual(InvocationExpressionSyntax syntax, Lazy<IMethodSymbol> methodSymbol) =>
            (syntax.IsKind(SyntaxKind.IdentifierName) || syntax.IsKind(SyntaxKind.ThisExpression)) &&
            (methodSymbol.Value.IsOverride && methodSymbol.Value.IsVirtual || methodSymbol.Value.IsVirtual);
    }
}
