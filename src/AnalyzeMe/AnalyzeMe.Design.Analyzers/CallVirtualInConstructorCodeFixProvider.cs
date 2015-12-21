using System.Collections.Immutable;
using System.Composition;
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

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var n = root.FindNode(diagnostic.Location.SourceSpan);
            var declaration = root.FindToken(diagnosticSpan.Start);
            var s = context.Document.GetSemanticModelAsync().Result.GetSymbolInfo(n);
            var ds = context.Document.Project.Solution.Projects
                .SelectMany(x => x.Documents
                    .Select(z => s.Symbol.Locations
                        .Where(l => l.SourceTree.FilePath == z.FilePath)))
                .ToArray();
            var locs = s.Symbol.Locations
                .Select(async x =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)x.SourceTree.GetRoot().FindNode(x.SourceSpan);
                    var newMethodDeclaration = methodDeclaration
                        .WithModifiers(methodDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.SealedKeyword)));
                    var document = context.Document.Project.Solution.Projects
                        .SelectMany(y => y.Documents)
                        .Single(y => y.Name == x.SourceTree.FilePath);
                    var sRoot = await document.GetSyntaxRootAsync();
                    sRoot = sRoot.ReplaceNode(methodDeclaration, newMethodDeclaration);
                    document = document.WithSyntaxRoot(sRoot);

                    return document;
                })
                .ToArray();

            await Task.WhenAll(locs)
                .ContinueWith(x =>
                {
                    foreach (var v in x.Result)
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create("Mark as sealed", c => Task.FromResult(v), "MarkMethodWithSealedModifier"),
                            diagnostic);
                    }
                });
        }
    }
}
