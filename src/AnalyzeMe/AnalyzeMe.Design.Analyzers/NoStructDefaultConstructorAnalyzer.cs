using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using AnalyzeMe.Design.Analyzers.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Semantics;

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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoStructDefaultConstructorDescription);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeStructDefaultCtor, SyntaxKind.ObjectCreationExpression);
            //context.RegisterSyntaxNodeAction(AnalyzeStructDefaultCtor, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeStructDefaultCtor(SyntaxNodeAnalysisContext ctx)
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


            var q = 1;
        }
    }

    public struct A
    {
        public A(int i)
        {
            
        }
    }
}
