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
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RxSubscribeMethodAnalyzer.RxSubscribeMethodDiagnosticId);
        private SyntaxTrivia WhiteSpace => SyntaxFactory.Whitespace(" ");
        private string MethodBodyComment => @"/*TODO: handle this!*/";

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var subscribeMethodInvocation = root.FindNode(diagnosticSpan) as InvocationExpressionSyntax;

            if (subscribeMethodInvocation == null)
            {
                return;
            }

            var methodArguments = subscribeMethodInvocation.ArgumentList;

            var newInvocationArguments = methodArguments.Arguments.First().NameColon != null
                ? CreateNamedArgumentsFrom(methodArguments)
                : CreateSimpleArgumentsFrom(methodArguments);
            var updatedRoot = root.ReplaceNode(methodArguments, newInvocationArguments);
            var updatedDocument = context.Document.WithSyntaxRoot(updatedRoot);

            context.RegisterCodeFix(
                CodeAction.Create("Add onError parameter", c => Task.FromResult(updatedDocument), "AddSubscribeOnErrorParameter"),
                diagnostic);
        }

        private ArgumentListSyntax CreateNamedArgumentsFrom(ArgumentListSyntax oldArguments)
        {
            var onErrorNameColon = SyntaxFactory.NameColon("onError");
            var onErrorArgument = CreateOnErrorLambdaArgument(onErrorNameColon);

            return oldArguments.AddArguments(onErrorArgument);
        }

        private ArgumentListSyntax CreateSimpleArgumentsFrom(ArgumentListSyntax oldArguments)
        {
            var onNextArgument = oldArguments.Arguments.First();
            var afterOnNextArguments = oldArguments.Arguments.Skip(1);
            //var a = afterOnNextArguments.FirstOrDefault()?.GetLeadingTrivia().
            var onErrorArgument = CreateOnErrorLambdaArgument();
            var subscribeMethodArguments = new[]
            {
                onNextArgument,
                onErrorArgument
            }.Union(afterOnNextArguments);

            var onNextCommaToken = oldArguments
                .ChildTokens()
                .FirstOrDefault(t => t.IsKind(SyntaxKind.CommaToken));
            var onErrorCommaToken = SyntaxFactory
                .Token(SyntaxKind.CommaToken)
                .WithTriviaFrom(onNextCommaToken);
            var otherArgumentCommaTokens = oldArguments
                .ChildTokens()
                .Where(t => t.IsKind(SyntaxKind.CommaToken))
                .Skip(1);

            var argumentCommaTokens = new[]
            {
                onNextCommaToken,
                onErrorCommaToken
            }.Union(otherArgumentCommaTokens);

            return oldArguments.WithArguments(SyntaxFactory.SeparatedList(subscribeMethodArguments, argumentCommaTokens));
        }

        private ArgumentSyntax CreateOnErrorLambdaArgument(NameColonSyntax nameColon = null)
        {
            var onErrorNameColon = nameColon;
            var parameter = SyntaxFactory
                .Parameter(SyntaxFactory.Identifier("ex"))
                .WithLeadingTrivia(WhiteSpace);
            var lambdaBodyToken = SyntaxFactory.Token
                (
                    SyntaxFactory.TriviaList(),
                    SyntaxKind.OpenBraceToken,
                    SyntaxFactory.TriviaList(WhiteSpace, SyntaxFactory.Comment(MethodBodyComment), WhiteSpace)
                );
            var lambdaBody = SyntaxFactory.Block(lambdaBodyToken, default(SyntaxList<StatementSyntax>), SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
            var lambdaExpression = SyntaxFactory.SimpleLambdaExpression(parameter, lambdaBody)
                .WithArrowToken(
                    SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken)
                        .WithLeadingTrivia(WhiteSpace)
                        .WithTrailingTrivia(WhiteSpace));
            var nopToken = SyntaxFactory.Token(SyntaxKind.None);
            var onErrorArgument =
                SyntaxFactory
                .Argument(onErrorNameColon, nopToken, lambdaExpression)
                .WithLeadingTrivia(WhiteSpace)
                .WithAdditionalAnnotations(Formatter.Annotation);
            
            return onErrorArgument;
        }
    }
}
