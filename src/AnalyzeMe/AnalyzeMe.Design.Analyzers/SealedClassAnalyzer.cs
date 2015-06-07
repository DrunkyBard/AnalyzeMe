using System.Collections.Immutable;
using System.Linq;
using AnalyzeMe.Design.Analyzers.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzeMe.Design.Analyzers
{
    /// <summary>
    /// Checks whether the class mark sealed modifier.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SealedClassAnalyzer : DiagnosticAnalyzer
    {
        public const string ClassCanBeSealedDiagnosticId = "MissingSealedModifier";
        internal static readonly LocalizableString ClassCanBeSealedTitle = "Class can be marked as sealed.";
        internal static readonly LocalizableString ClassCanBeSealedMessageFormat = "Class can be marked as sealed.";
        internal const string AttributeUsageCategory = "Design";
        internal static readonly DiagnosticDescriptor ClassCanBeSealedRule = new DiagnosticDescriptor(
            ClassCanBeSealedDiagnosticId,
            ClassCanBeSealedTitle,
            ClassCanBeSealedMessageFormat,
            AttributeUsageCategory,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ClassCanBeSealedRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private async void AnalyzeClass(SyntaxNodeAnalysisContext ctx)
        {
            var classDeclarationNode = (ClassDeclarationSyntax)ctx.Node;
            var classDeclarationSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclarationNode);
            var workspace = classDeclarationNode.TryGetWorkspace();

            if (IsAbstractStaticOrSealed(classDeclarationSymbol) || workspace == null)
            {
                return;
            }

            var derivedTypes = await classDeclarationSymbol.FindDerivedClassesAsync(workspace.CurrentSolution, ctx.CancellationToken);

            if (!derivedTypes.Any())
            {
                var diagnostic = Diagnostic.Create(ClassCanBeSealedRule, classDeclarationNode.Identifier.GetLocation());
                ctx.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsAbstractStaticOrSealed(INamedTypeSymbol symbol)
        {
            var visitor = new ClassMembersVisitor();
            var classMembers = symbol.GetMembers().ToList();

            return symbol.IsStatic || symbol.IsSealed || symbol.IsAbstract || classMembers.Any(visitor.Visit);
        }

        private class ClassMembersVisitor : SymbolVisitor<bool>
        {
            public override bool VisitProperty(IPropertySymbol symbol)
            {
                return symbol.IsVirtual;
            }

            public override bool VisitMethod(IMethodSymbol symbol)
            {
                return symbol.IsVirtual;
            }
        }
    }
}