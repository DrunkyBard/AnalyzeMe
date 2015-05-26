using System;
using System.Collections.Immutable;
using System.Linq;
using AnalyzeMe.Design.Analyzers.Utils;
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
            //context.RegisterSymbolAction(AnalyzeClassSymbol, SymbolKind.c);
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClassSymbol(SymbolAnalysisContext ctx)
        {
        }

        private async void AnalyzeClass(SyntaxNodeAnalysisContext ctx)
        {
            var classDeclarationNode = (ClassDeclarationSyntax)ctx.Node;
            var syntaxTreeContainer = classDeclarationNode.SyntaxTree.GetText().Container;
            Workspace workspace;
            Workspace.TryGetWorkspace(syntaxTreeContainer, out workspace);

            if (workspace == null)
            {
                return;
            }

            var classDeclarationSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclarationNode);

            try
            {
                await classDeclarationSymbol.FindDerivedClassesAsync(workspace.CurrentSolution, ctx.CancellationToken); //!!!

            }
            catch (Exception e)
            {
            }

            //var sln = workspace.CurrentSolution;
            //try
            //{
            //    var visitor = new CustomSymbolVisitor();

            //    foreach (var project in sln.Projects)
            //    {
            //        var projectCompilation = await project.GetCompilationAsync();
            //        var symbols = projectCompilation.SyntaxTrees
            //            .SelectMany(x => x.GetRoot().DescendantNodes(y => y is ClassDeclarationSyntax))
            //            .Select(x => projectCompilation.GetSemanticModel(x.SyntaxTree).GetDeclaredSymbol(x))
            //            .ToList();

            //        var locationsInMetadata = classDeclarationSymbol.Locations.Any(loc => loc.IsInMetadata);

            //        if (true)
            //        {
            //            var t = projectCompilation.Assembly.TypeNames
            //                .Select(x => projectCompilation.GetTypeByMetadataName(x))
            //                .ToList();

            //            var l = projectCompilation.Assembly.GlobalNamespace.GetMembers().Where(x => x.IsType);
            //            var l1 = projectCompilation.GlobalNamespace.GetTypeMembers();

            //            foreach (var namespaceOrTypeSymbol in l)
            //            {
            //                namespaceOrTypeSymbol.Accept(visitor);
            //            }
            //        }
            //    }

            //    var references32 = await SymbolFinder.FindReferencesAsync(classDeclarationSymbol.AssociatedSymbol, sln);
            //    var references3 = await SymbolFinder.FindReferencesAsync(classDeclarationSymbol.ConstructedFrom, sln);
            //    var references43 = await SymbolFinder.FindReferencesAsync(classDeclarationSymbol.OriginalDefinition, sln);
            //    var references = await SymbolFinder.FindReferencesAsync(classDeclarationSymbol, sln);
            //    var refs = await SymbolFinder.FindImplementationsAsync(classDeclarationSymbol, sln, sln.Projects.ToImmutableSortedSet());


            //    foreach (var referencedSymbol in references)
            //    {
            //        referencedSymbol.Definition.Accept(visitor);
            //    }
            //    foreach (var referencedSymbol in references32)
            //    {
            //        referencedSymbol.Definition.Accept(visitor);
            //    }
            //    foreach (var referencedSymbol in references3)
            //    {
            //        referencedSymbol.Definition.Accept(visitor);
            //    }
            //    foreach (var referencedSymbol in references43)
            //    {
            //        referencedSymbol.Definition.Accept(visitor);
            //    }

            //    foreach (var symbol in refs)
            //    {
            //        symbol.Accept(visitor);
            //    }
            //}
            //catch (Exception)
            //{
            //    var g = 1;
            //}


            //var a = 1;
        }
    }

    class CustomSymbolVisitor : SymbolVisitor
    {
        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            var a = 1;
            base.VisitNamedType(symbol);
        }
    }
}