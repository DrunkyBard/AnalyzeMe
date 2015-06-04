using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace AnalyzeMe.Design.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SealedClassCodeFixProvider)), Shared]
    public class SealedClassCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SealedClassAnalyzer.ClassCanBeSealedDiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var declaration = (ClassDeclarationSyntax)root.FindToken(diagnosticSpan.Start).Parent;
            var sealedDeclaration = declaration
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.SealedKeyword))
                .WithAdditionalAnnotations(Formatter.Annotation);
            var updatedRoot = root.ReplaceNode(declaration, sealedDeclaration);
            var updatedDocument = context.Document.WithSyntaxRoot(updatedRoot);

            context.RegisterCodeFix(
                CodeAction.Create("Mark as sealed", c => Task.FromResult(updatedDocument)),
                diagnostic);
        }
    }
}
