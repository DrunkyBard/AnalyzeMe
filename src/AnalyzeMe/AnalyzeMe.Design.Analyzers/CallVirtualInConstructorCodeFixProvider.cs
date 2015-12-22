using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CallVirtualInConstructorCodeFixProvider)), Shared]
    internal class CallVirtualInConstructorCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(CallVirtualInConstructorAnalyzer.MethodCanBeMarkedAsSealedId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        //TODO: If diagnostic in derived type, and derived type dont override virtual method, and this type doesnt have sub types, then mark this type as sealed.
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticNode = root.FindNode(diagnostic.Location.SourceSpan);
            var diagnosticSymbolInfo = context.Document.GetSemanticModelAsync().Result.GetSymbolInfo(diagnosticNode);
            var fixedDocuments = diagnosticSymbolInfo.Symbol.Locations
                .Select(async x =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)x.SourceTree.GetRoot().FindNode(x.SourceSpan);
                    var newMethodDeclaration = methodDeclaration
                        .WithModifiers(methodDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.SealedKeyword)));
                    var document = context.Document.Project.Solution.Projects
                        .SelectMany(y => y.Documents)
                        .Single(y => y.Name == Path.GetFileName(x.SourceTree.FilePath));
                    var documentSyntaxRoot = await document.GetSyntaxRootAsync();
                    documentSyntaxRoot = documentSyntaxRoot.ReplaceNode(methodDeclaration, newMethodDeclaration);
                    document = document.WithSyntaxRoot(documentSyntaxRoot);

                    return document;
                })
                .ToArray();

            await Task.WhenAll(fixedDocuments)
                .ContinueWith(x =>
                {
                    foreach (var v in x.Result)
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create("Mark method as sealed", _ => Task.FromResult(v), "MarkMethodWithSealedModifier"),
                            diagnostic);
                    }
                });
        }
    }
}
