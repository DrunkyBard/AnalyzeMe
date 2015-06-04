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
            var classMembers = classDeclarationSymbol.GetMembers().ToList();
            var visitor = new ClassMembersVisitor();
            
            if (classDeclarationSymbol.IsSealed || classDeclarationSymbol.IsAbstract || classMembers.Any(visitor.Visit))
            {
                return;
            }

            var syntaxTreeContainer = classDeclarationNode.SyntaxTree.GetText().Container;
            Workspace workspace;
            Workspace.TryGetWorkspace(syntaxTreeContainer, out workspace);

            if (workspace == null)
            {
                return;
            }

            var derivedTypes = await classDeclarationSymbol.FindDerivedClassesAsync(workspace.CurrentSolution, ctx.CancellationToken);

            if (!derivedTypes.Any())
            {
                var diagnostic = Diagnostic.Create(ClassCanBeSealedRule, classDeclarationNode.Identifier.GetLocation());
                //var diagnostic = Diagnostic.Create(ClassCanBeSealedRule, classDeclarationNode.GetLocation());
                ctx.ReportDiagnostic(diagnostic);
            }
        }
    }

    class ClassMembersVisitor : SymbolVisitor<bool>
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