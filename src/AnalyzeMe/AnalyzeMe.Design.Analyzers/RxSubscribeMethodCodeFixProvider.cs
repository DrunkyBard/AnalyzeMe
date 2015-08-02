using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RxSubscribeMethodCodeFixProvider)), Shared]
    public sealed class RxSubscribeMethodCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RxSubscribeMethodAnalyzer.RxSubscribeMethodDiagnosticId);

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

            var a = 1;
        }

        private ArgumentListSyntax CreateNamedArgumentsFrom(ArgumentListSyntax oldArguments)
        {
            var onErrorNameColon = SyntaxFactory.NameColon("onError");
            var nopToken = SyntaxFactory.Token(SyntaxKind.None);
            var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("e"));
            var lambdaBody = SyntaxFactory.Block()
                                                    .WithOpenBraceToken
                                                    (
                                                        SyntaxFactory.Token
                                                        (
                                                            SyntaxFactory.TriviaList(),
                                                            SyntaxKind.OpenBraceToken,
                                                            SyntaxFactory.TriviaList
                                                            (
                                                                SyntaxFactory.Comment
                                                                (
                                                                    @"/*TODO: handle*/"
                                                                )
                                                            )
                                                        )
                                                    )
                                                    .WithCloseBraceToken
                                                    (
                                                        SyntaxFactory.Token
                                                        (
                                                            SyntaxKind.CloseBraceToken
                                                        )
                                                    );
            var lambdaExpression = SyntaxFactory.SimpleLambdaExpression(parameter, lambdaBody)
                .WithArrowToken(SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken));
            var onErrorArgument = SyntaxFactory.Argument(onErrorNameColon, nopToken, lambdaExpression);

            return oldArguments.AddArguments(onErrorArgument);
        }

        private ArgumentListSyntax CreateSimpleArgumentsFrom(ArgumentListSyntax oldArguments)
        {
            return null;
        }
    }
}
