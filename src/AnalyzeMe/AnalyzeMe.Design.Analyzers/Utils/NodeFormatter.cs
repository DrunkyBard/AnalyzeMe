using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static class NodeFormatter
    {
        public static ArgumentSyntax Format(this ArgumentSyntax argument)
        {
            var argumentListOption = argument.Parent.As<ArgumentListSyntax>();

            if (!argumentListOption.HasValue)
            {
                return argument;
            }

            var argumentList = argumentListOption.Value;
            var argIdx = argumentList.Arguments.IndexOf(argument);
            
            if (argIdx == 0)
            {
                return FormatFirst(argumentList, argument);
            }

            if (argIdx == argumentList.Arguments.Count - 1)
            {
                return FormatLast(argumentList, argument);
            }

            var previousArg = argumentList.Arguments[argIdx - 1];
            var nextArg = argumentList.Arguments[argIdx + 1];

            return FormatBetween(previousArg, argument, nextArg);
        }

        private static ArgumentSyntax FormatFirst(ArgumentListSyntax arguments, ArgumentSyntax formattedArgument)
        {
            var nextComma = arguments.Arguments[0].GetNextComma();
            var nextArg = arguments.Arguments[0].TryGetNextArgument();
            var argumentStartsWithNewLine = arguments.OpenParenToken.GetStartLinePosition() != nextArg.Value.GetStartLinePosition();
            var nextCommaLastTrailingTrivia = !nextArg.HasValue
                ? new SyntaxTrivia()
                : (!argumentStartsWithNewLine
                    ? new SyntaxTrivia()
                    : SyntaxFactory.EndOfLine(Environment.NewLine));
            var openParenTrailingTrivia = argumentStartsWithNewLine 
                ? SyntaxFactory.EndOfLine(Environment.NewLine) 
                : new SyntaxTrivia();
            var formattedComma = nextComma
                .WithoutLeadingTrivia(SyntaxKind.WhitespaceTrivia)
                .ReplaceTrivia(t => t.TrailingTrivia.LastOrDefault(x => x.IsKind(SyntaxKind.EndOfLineTrivia)), nextCommaLastTrailingTrivia)
                .WithoutTrailingTrivia(SyntaxKind.WhitespaceTrivia)
                .AddToTheTopTrailingTrivia(SyntaxFactory.Whitespace(" "));

            arguments = arguments
                .ReplaceToken(
                    arguments.OpenParenToken, 
                    arguments.OpenParenToken
                        .WithoutTrailingTrivia(SyntaxKind.EndOfLineTrivia, SyntaxKind.WhitespaceTrivia)
                        .AppendTrailingTrivia(openParenTrailingTrivia))
                .ReplaceToken(n => n.Arguments[0].GetNextComma(), formattedComma)
                .ReplaceNode(n => n.Arguments[0], arguments.Arguments[0].WithoutTrailingTrivia(SyntaxKind.WhitespaceTrivia, SyntaxKind.EndOfLineTrivia))
                .ReplaceNode(n => n.Arguments[0], n => AlignWith(n, n.TryGetNextArgument()));

            if (arguments.Arguments.Count > 1)
            {
                if (arguments.OpenParenToken.GetStartLinePosition() != arguments.Arguments[1].GetStartLinePosition() &&
                    arguments.Arguments[1].GetPreviousComma().GetStartLinePosition() == arguments.Arguments[1].GetStartLinePosition())
                {
                    var secondArgumentWhitespaceLength = arguments.Arguments[1].ExtractWhitespace().Span.Length;
                    formattedArgument = formattedArgument.WithoutTrivia(SyntaxKind.WhitespaceTrivia);
                    var formattedArgumentLeadingTriviaLength = formattedArgument.GetLeadingTrivia().Span.Length;

                    if (formattedArgumentLeadingTriviaLength < secondArgumentWhitespaceLength)
                    {
                        var newWhitespace = string.Concat(Enumerable.Repeat(" ", secondArgumentWhitespaceLength - formattedArgumentLeadingTriviaLength));
                        var newLeadingTrivias = SyntaxFactory.TriviaList(
                            formattedArgument.GetLeadingTrivia()
                                .Union(SyntaxTriviaList.Create(SyntaxFactory.Whitespace(newWhitespace))));
                        formattedArgument = formattedArgument.WithLeadingTrivia(newLeadingTrivias);
                        var commaTrailingTrivia = arguments.Arguments[1].GetPreviousComma().TrailingTrivia;
                        var eolComma = arguments.Arguments[1]
                            .GetPreviousComma()
                            .WithTrailingTrivia(
                                SyntaxTriviaList.Create(SyntaxFactory.EndOfLine(Environment.NewLine)).Union(commaTrailingTrivia));
                        arguments = arguments
                            .ReplaceNode(n => n.Arguments[0], formattedArgument)
                            .ReplaceToken(n => n.Arguments[1].GetPreviousComma(), eolComma);
                    }
                }
                else
                {
                    formattedArgument = formattedArgument.WithoutFirstLeadingTrivia(SyntaxKind.WhitespaceTrivia);
                    var newSecondArgumentSeparator =
                        arguments.Arguments[1].GetPreviousComma()
                            .WithoutFirstTrailingTrivia(SyntaxKind.WhitespaceTrivia)
                            .WithTrailingTrivia(SyntaxFactory.Whitespace(" "));
                    arguments = arguments
                        .ReplaceNode(n => n.Arguments[0], formattedArgument)
                        .ReplaceToken(
                            t => t.OpenParenToken,
                            arguments.OpenParenToken.WithoutFirstTrailingTrivia(SyntaxKind.WhitespaceTrivia))
                        .ReplaceToken(
                            t => t.Arguments[1].GetPreviousComma(),
                            newSecondArgumentSeparator
                        )
                        .ReplaceNode(
                            n => n.Arguments[1],
                            arguments.Arguments[1].WithoutFirstLeadingTrivia(SyntaxKind.WhitespaceTrivia));
                }
            }
            else
            {
                if (arguments.OpenParenToken.GetStartLinePosition() == formattedArgument.GetStartLinePosition())
                {
                    formattedArgument = formattedArgument.WithoutTrivia(SyntaxKind.WhitespaceTrivia);
                    arguments = arguments.ReplaceNode(arguments.Arguments[0], formattedArgument);
                }
            }

            return arguments.Arguments[0];
        }

        private static ArgumentSyntax AlignWith(ArgumentSyntax argument, Optional<ArgumentSyntax> alignedArgument)
        {
            if (!alignedArgument.HasValue || argument.GetStartLinePosition() == alignedArgument.Value.GetStartLinePosition())
            {
                return argument;
            }

            argument = argument.WithoutLeadingTrivia(SyntaxKind.WhitespaceTrivia);
            var firstArgumentLeadingTriviaLength = argument.GetLeadingTrivia().Span.Length;
            var secondArgumentWhitespaceLength = alignedArgument.Value.ExtractWhitespace().Span.Length;

            if (firstArgumentLeadingTriviaLength > secondArgumentWhitespaceLength)
            {
                return argument; // For case when formatted argument contains comment leading trivia, etc.
            }

            var leadingWhitespace = string.Concat(Enumerable.Repeat(" ", secondArgumentWhitespaceLength - firstArgumentLeadingTriviaLength));

            return argument.AddLeadingTrivia(SyntaxFactory.Whitespace(leadingWhitespace));
        }

        // Рассмотреть вариант: просто повторить все leading/trailing trivia предыдущих/последующих запятых/аргументов
        private static ArgumentSyntax FormatLast(ArgumentListSyntax arguments, ArgumentSyntax formattedArgument)
        {
            throw new NotImplementedException();
        }

        private static ArgumentSyntax FormatBetween(ArgumentSyntax previousArgument, ArgumentSyntax formattedArgument, ArgumentSyntax nextArgument)
        {
            throw new NotImplementedException();
        }
    }
}
