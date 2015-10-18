using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using AnalyzeMe.Design.Analyzers.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            var newInvocationArguments = methodArguments.Arguments.Any(arg => arg.NameColon != null)
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
            var lastArgument = oldArguments.Arguments.Last();
            var onErrorNameColon = SyntaxFactory.NameColon("onError");
            var onErrorArgument = CreateOnErrorLambdaArgument(onErrorNameColon);
            onErrorArgument = FormatOnErrorArgument(onErrorArgument, lastArgument);
            var lastComma = lastArgument.GetAssociatedComma();
            var hasEol = lastComma.TrailingTrivia.Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia));

            var eolTrivia = hasEol || (oldArguments.Arguments.Count == 1 && oldArguments.OpenParenToken.TrailingTrivia.Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
                ? SyntaxFactory.EndOfLine(Environment.NewLine)
                : SyntaxFactory.Whitespace(String.Empty);
            var onErrorComma = SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.CommaToken, SyntaxTriviaList.Create(eolTrivia));
            var newArguments = oldArguments
                .Arguments
                .Take(oldArguments.Arguments.Count - 1)
                .Union(new[] { lastArgument.WithoutEolTrivia(), onErrorArgument });
            var newCommas = oldArguments.Arguments.GetSeparators().ToList();
            newCommas.Add(onErrorComma);

            return oldArguments.WithArguments(SyntaxFactory.SeparatedList(newArguments, newCommas));
        }

        private ArgumentListSyntax CreateSimpleArgumentsFrom(ArgumentListSyntax oldArguments)
        {
            //return CreateOnErrorLambdaArgument()
            //    .InsertAfter(oldArguments.Arguments.First())
            //    .Format()
            //    .Parent
            //    .As<ArgumentListSyntax>()
            //    .Value;


            var onNextArgument = oldArguments.Arguments.First();
            var afterOnNextArguments = oldArguments.Arguments.Skip(1).ToArray();
            var firstAfterOnNextArg = afterOnNextArguments.FirstOrDefault();
            var onErrorArgument = FormatOnErrorArgument(CreateOnErrorLambdaArgument(), onNextArgument, firstAfterOnNextArg);
            var afterOnNextCommaToken = SyntaxFactory.Token(SyntaxKind.None);
            SyntaxTriviaList trailingCommaTokenTrivia = SyntaxTriviaList.Empty;

            if (firstAfterOnNextArg == null)
            {
                var openParenAndOnNextArgInOneLine = onNextArgument
                    .Parent
                    .As<ArgumentListSyntax>()
                    .Value
                    .OpenParenToken
                    .GetStartLinePosition() == onNextArgument.GetStartLinePosition();
                var onNextArgInOneLine = onNextArgument.GetStartLinePosition() == onNextArgument.GetEndLinePosition();

                if (!openParenAndOnNextArgInOneLine || !onNextArgInOneLine)
                {
                    onNextArgument = onNextArgument.WithoutEolTrivia();
                    trailingCommaTokenTrivia = trailingCommaTokenTrivia.Add(SyntaxFactory.EndOfLine(Environment.NewLine));
                }
                //else
                //{
                //    trailingCommaTokenTrivia = trailingCommaTokenTrivia.Add(WhiteSpace);
                //}
            }
            else
            {
                afterOnNextCommaToken = firstAfterOnNextArg.GetAssociatedComma();

                SyntaxTriviaList a;

                if (afterOnNextCommaToken.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia))
                {
                    a = new SyntaxTriviaList().Add(SyntaxFactory.EndOfLine(Environment.NewLine));
                }
                else
                {
                    a = afterOnNextCommaToken.TrailingTrivia;
                    afterOnNextCommaToken = afterOnNextCommaToken.WithTrailingTrivia(WhiteSpace);
                }

                //var a = afterOnNextCommaToken.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia)
                //    ? SyntaxFactory.EndOfLine(Environment.NewLine)
                //    : WhiteSpace;
                //trailingCommaTokenTrivia = new SyntaxTriviaList().Add(a);
                trailingCommaTokenTrivia = a;
            }

            var onErrorCommaToken = SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.CommaToken, trailingCommaTokenTrivia);
            var otherArgumentCommaTokens = oldArguments.Arguments.GetSeparators().Skip(1);
            var tList = new List<SyntaxToken>();

            if (!afterOnNextCommaToken.IsKind(SyntaxKind.None))
            {
                tList.Add(afterOnNextCommaToken);
            }
            tList.Add(onErrorCommaToken);

            tList.AddRange(otherArgumentCommaTokens);

            var subscribeMethodArguments = new[]
            {
                onNextArgument,
                onErrorArgument
            }.Union(afterOnNextArguments);

            return oldArguments.WithArguments(SyntaxFactory.SeparatedList(subscribeMethodArguments, tList));
        }

        private ArgumentSyntax FormatOnErrorArgument(ArgumentSyntax onErrorArgument, ArgumentSyntax onNextArgument, ArgumentSyntax firstArgumentAfterOnNext = null)
        {
            SyntaxTrivia whitespace = SyntaxFactory.Whitespace(String.Empty);

            if (firstArgumentAfterOnNext != null)
            {
                if (firstArgumentAfterOnNext.GetAssociatedComma().TrailingTrivia.Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
                {
                    whitespace = firstArgumentAfterOnNext.Expression.ExtractWhitespace();
                }

                return onErrorArgument.WithLeadingTrivia(whitespace);
            }

            var trailingEol = onNextArgument.GetTrailingTrivia().LastOrDefault().IsKind(SyntaxKind.EndOfLineTrivia)
                ? SyntaxFactory.EndOfLine(Environment.NewLine)
                : SyntaxFactory.Whitespace(String.Empty);

            whitespace = onNextArgument.Expression.ExtractWhitespace();

            return onErrorArgument.WithLeadingTrivia(whitespace).WithTrailingTrivia(trailingEol);
        }

        private ArgumentSyntax CreateOnErrorLambdaArgument(NameColonSyntax nameColon = null)
        {
            var onErrorNameColon = nameColon;
            var parameter = SyntaxFactory
                .Parameter(SyntaxFactory.Identifier(SyntaxTriviaList.Empty, "ex", SyntaxTriviaList.Empty))
                .WithTrailingTrivia(WhiteSpace);
            var lambdaBodyToken = SyntaxFactory.Token
                (
                    SyntaxFactory.TriviaList(),
                    SyntaxKind.OpenBraceToken,
                    SyntaxFactory.TriviaList(WhiteSpace, SyntaxFactory.Comment(MethodBodyComment), WhiteSpace)
                );
            var lambdaBody = SyntaxFactory.Block(
                lambdaBodyToken,
                default(SyntaxList<StatementSyntax>),
                SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.CloseBraceToken, SyntaxTriviaList.Empty)); // Specified SyntaxTriviaList.Empty, because overload with SyntaxKind parameter insert
                                                                                                                  // elastic trivia and then syntax formatting will replace elastic markers with appropriate trivia.
            var lambdaExpression = SyntaxFactory.SimpleLambdaExpression(parameter, lambdaBody)
                .WithArrowToken(
                    SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.EqualsGreaterThanToken, SyntaxTriviaList.Empty)
                        .WithTrailingTrivia(WhiteSpace));
            var nopToken = SyntaxFactory.Token(SyntaxKind.None);
            var onErrorArgument = SyntaxFactory.Argument(onErrorNameColon, nopToken, lambdaExpression);

            return onErrorArgument;
        }
    }
}
