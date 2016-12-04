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
    public sealed class NoStructDefaultConstructorAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor NoStructDefaultConstructorDescription = new DiagnosticDescriptor(
            "NoStructDefaultConstructorId",
            "",
            "",
            "",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        internal static readonly DiagnosticDescriptor StructHasAttributeButHasParameterlessConstructorOnly = new DiagnosticDescriptor(
            "StructHasAttributeButHasParameterlessConstructorOnlyId",
            "",
            "",
            "",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoStructDefaultConstructorDescription, StructHasAttributeButHasParameterlessConstructorOnly);

        public override void Initialize(AnalysisContext context)
        {
            //context.RegisterSyntaxNodeAction(AnalyzeStructDefaultCtorInvocation, SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeStructTypeParameter, SyntaxKind.GenericName);
            //context.RegisterSyntaxNodeAction(AnalyzeStructDefaultCtorInvocation, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeStructTypeParameter(SyntaxNodeAnalysisContext ctx)
        {
            var visitor = new GenericInvocationVisitor();
            ctx
                .Node
                .As<CSharpSyntaxNode>()
                .Value
                .Accept(visitor)
                .WhenHasValueThen(ctx.ReportDiagnostic);
        }

        private void AnalyzeStructDefaultCtorInvocation(SyntaxNodeAnalysisContext ctx)
        {
            var objectCreation = ctx
                .Node
                .As<ObjectCreationExpressionSyntax>()
                .Value;

            if (objectCreation.ArgumentList.Arguments.Any())
            {
                return;
            }

            var symbolInfo = ctx.SemanticModel.GetSymbolInfo(objectCreation.Type);

            var namedSymbol = symbolInfo.Symbol?.As<INamedTypeSymbol>().Value;

            if (namedSymbol == null || !namedSymbol.IsValueType)
            {
                return;
            }

            var hasAttribute = namedSymbol
                .DeclaringSyntaxReferences
                .Select(x => x.GetSyntax())
                .OfType<StructDeclarationSyntax>()
                .Any(structDeclaration => structDeclaration
                .AttributeLists
                .SelectMany(x => x.Attributes)
                .Any(x =>
                {
                    var attrSymbolInfo = ctx.SemanticModel.GetSymbolInfo(x, ctx.CancellationToken);

                    return attrSymbolInfo.Symbol != null &&
                           ctx.SemanticModel
                              .GetSymbolInfo(x, ctx.CancellationToken).Symbol
                              .As<IMethodSymbol>()
                              .Value
                              .ReceiverType
                              .OriginalDefinition
                              .ToString()
                              .Equals("AnalyzeMe.WorkProcess.Tools.NoDefaultConstructorAttribute", StringComparison.Ordinal);
                }));

            var hasDefaultConstructorOnly = !namedSymbol.Constructors.Any(x => x.Arity > 0);

            if (hasAttribute)
            {
                var d = Diagnostic.Create(NoStructDefaultConstructorDescription, ctx.Node.GetLocation());
                ctx.ReportDiagnostic(d);

                if (hasDefaultConstructorOnly)
                {
                    var diagnostic = Diagnostic.Create(StructHasAttributeButHasParameterlessConstructorOnly, ctx.Node.GetLocation());
                    ctx.ReportDiagnostic(diagnostic);
                }
            }


            var q = 1;
        }

        private sealed class GenericInvocationVisitor : CSharpSyntaxVisitor<Optional<Diagnostic>>
        {
            public override Optional<Diagnostic> VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
            {
                return base.VisitObjectCreationExpression(node);
            }

            public override Optional<Diagnostic> VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                return base.VisitInvocationExpression(node);
            }

            public override Optional<Diagnostic> DefaultVisit(SyntaxNode node)
            {
                throw new InvalidOperationException();
            }
        }
    }

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
}
