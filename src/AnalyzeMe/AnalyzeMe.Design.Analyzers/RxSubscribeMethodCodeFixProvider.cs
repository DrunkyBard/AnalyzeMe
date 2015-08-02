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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RxSubscribeMethodCodeFixProvider)), Shared]
    public sealed class RxSubscribeMethodCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RxSubscribeMethodAnalyzer.RxSubscribeMethodDiagnosticId);
        private SyntaxTrivia WhiteSpace => SyntaxFactory.Whitespace(" ");

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var subscribeMethodInvocation = (InvocationExpressionSyntax)root.FindNode(diagnosticSpan);

            var methodArguments = subscribeMethodInvocation.ArgumentList;
            var newInvocationArguments = methodArguments.Arguments.First().NameColon != null
                ? CreateNamedArgumentsFrom(methodArguments)
                : CreateSimpleArgumentsFrom(methodArguments);
            var updatedRoot = root.ReplaceNode(methodArguments, newInvocationArguments.WithAdditionalAnnotations(Formatter.Annotation));
            var updatedDocument = context.Document.WithSyntaxRoot(updatedRoot);

            context.RegisterCodeFix(
                CodeAction.Create("Add onError parameter.", c => Task.FromResult(updatedDocument), "AddSubscribeOnErrorParameter"),
                diagnostic);
        }

        private ArgumentListSyntax CreateNamedArgumentsFrom(ArgumentListSyntax oldArguments)
        {
            var onErrorNameColon = SyntaxFactory.NameColon("onError");
            var nopToken = SyntaxFactory.Token(SyntaxKind.None);
            var parameter = SyntaxFactory
                .Parameter(SyntaxFactory.Identifier("e"))
                .WithLeadingTrivia(SyntaxFactory.Whitespace(" "));
            var lambdaBodyToken = SyntaxFactory.Token
                (
                    SyntaxFactory.TriviaList(),
                    SyntaxKind.OpenBraceToken,
                    SyntaxFactory.TriviaList
                        (
                            WhiteSpace,
                            SyntaxFactory.Comment
                                (
                                    @"/*TODO: handle this!*/"
                                ),
                            WhiteSpace
                        )
                );
            var lambdaBody = SyntaxFactory.Block()
                .WithOpenBraceToken(lambdaBodyToken)
                .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
            var lambdaExpression = SyntaxFactory.SimpleLambdaExpression(parameter, lambdaBody)
                .WithArrowToken(
                    SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken)
                        .WithLeadingTrivia(WhiteSpace)
                        .WithTrailingTrivia(WhiteSpace));
            var onErrorArgument = 
                SyntaxFactory
                .Argument(onErrorNameColon, nopToken, lambdaExpression)
                .WithLeadingTrivia(WhiteSpace);

            return oldArguments.AddArguments(onErrorArgument);
        }

        private ArgumentListSyntax CreateSimpleArgumentsFrom(ArgumentListSyntax oldArguments)
        {
            return null;
        }
    }
}
