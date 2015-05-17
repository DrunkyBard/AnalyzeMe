using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;

namespace AnalyzeMe.Design.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SealedClassAnalyzer : DiagnosticAnalyzer
    {
        public const string ClassCanBeSealedDiagnosticId = "MissingSealedModifier";
        internal static readonly LocalizableString ClassCanBeSealedTitle = "Class can be marked as sealed.";
        internal static readonly LocalizableString ClassCanBeSealedMessageFormat = "Class can be marked as sealed.";
        internal const string AttributeUsageCategory = "Design";
        internal static readonly DiagnosticDescriptor ClassCanBeSealedRule = new DiagnosticDescriptor(ClassCanBeSealedDiagnosticId, ClassCanBeSealedTitle, ClassCanBeSealedMessageFormat, AttributeUsageCategory, DiagnosticSeverity.Error, isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ClassCanBeSealedRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private async void AnalyzeClass(SyntaxNodeAnalysisContext ctx)
        {
            var node = (ClassDeclarationSyntax) ctx.Node;
            Workspace ws;
            Workspace.TryGetWorkspace(node.SyntaxTree.GetText().Container, out ws);
            INamedTypeSymbol symbol = ctx.SemanticModel.GetDeclaredSymbol(node);
            var references = await SymbolFinder.FindReferencesAsync(symbol, ws.CurrentSolution);
            var visitor = new ClassDeclarationVisitor();
            var a1 = node.Members
                .Any(x => x.Accept(visitor));
            var a = 1;
        }
    }

    public class ClassDeclarationVisitor : CSharpSyntaxVisitor<bool>
    {
        private readonly Func<SyntaxToken, bool> _virtualTokenComparator;

        public ClassDeclarationVisitor()
        {
            _virtualTokenComparator = token => token.ValueText == "virtual";
        }

        public override bool VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            return node
                .Modifiers
                .Any(_virtualTokenComparator);
        }

        public override bool VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            return node
                .Modifiers
                .Any(_virtualTokenComparator);
        }
    }
}