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

            return FormatBetween(argumentList, argIdx - 1, argIdx, argIdx + 1);
        }

        private static ArgumentSyntax FormatFirst(ArgumentListSyntax arguments, ArgumentSyntax formattedArgument)
        {
            var nextComma = formattedArgument.GetNextComma();
            var nextArg = formattedArgument.TryGetNextArgument();
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
                .WithoutTrailingTrivia(SyntaxKind.WhitespaceTrivia)
                .AddToTheTopTrailingTrivia(SyntaxFactory.Whitespace(" "))
                .ReplaceTrivia(t => t.TrailingTrivia.LastOrDefault(x => x.IsKind(SyntaxKind.EndOfLineTrivia)), nextCommaLastTrailingTrivia);

            arguments = arguments
                .ReplaceToken(
                    arguments.OpenParenToken, 
                    arguments.OpenParenToken
                        .WithoutTrailingTrivia(SyntaxKind.EndOfLineTrivia, SyntaxKind.WhitespaceTrivia)
                        .AppendTrailingTrivia(openParenTrailingTrivia))
                .ReplaceNode(
                        n => n.Arguments[0], 
                        arguments.Arguments[0]
                            .WithoutTrivia(SyntaxKind.WhitespaceTrivia)
                            .WithoutTrailingTrivia(SyntaxKind.EndOfLineTrivia))
                //.ReplaceNode(n => n.Arguments[0], arguments.Arguments[0].WithoutTrailingTrivia(SyntaxKind.WhitespaceTrivia, SyntaxKind.EndOfLineTrivia))
                .ReplaceNode(n => n.Arguments[0], n => AlignWith(n, n.TryGetNextArgument()))
                .ReplaceToken(n => n.Arguments[0].GetNextComma(), formattedComma);

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

            if (firstArgumentLeadingTriviaLength > secondArgumentWhitespaceLength) // For case when formatted argument contains comment leading trivia, etc.
            {
                return argument; 
            }

            var leadingWhitespace = string.Concat(Enumerable.Repeat(" ", secondArgumentWhitespaceLength - firstArgumentLeadingTriviaLength));

            return argument.AddLeadingTrivia(SyntaxFactory.Whitespace(leadingWhitespace));
        }

        private static ArgumentSyntax FormatLast(ArgumentListSyntax arguments, ArgumentSyntax formattedArgument)
        {
            var argumentsCount = arguments.Arguments.Count;
            var previousArgument = arguments.Arguments[argumentsCount - 2];
            var previousArgumentStartsWithNewLine = arguments.OpenParenToken.GetStartLinePosition() != previousArgument.GetStartLinePosition();
            var lastCommaTrailingTrivia = previousArgumentStartsWithNewLine
                ? SyntaxFactory.EndOfLine(Environment.NewLine)
                : new SyntaxTrivia();
            var lastOriginComma = formattedArgument.GetPreviousComma();
            var lastFormattedComma = lastOriginComma
                .WithoutTrivia(SyntaxKind.WhitespaceTrivia)
                .WithoutTrailingTrivia(SyntaxKind.EndOfLineTrivia)
                .AddToTheTopTrailingTrivia(SyntaxFactory.Whitespace(" "))
                .AppendTrailingTrivia(lastCommaTrailingTrivia);

            arguments = arguments
                .ReplaceToken(n => formattedArgument.GetPreviousComma(), lastFormattedComma)
                .ReplaceNode(n => n.Arguments.Last(), arg => AlignWith(arg, previousArgument));

            return arguments.Arguments.Last();
        }

        private static ArgumentSyntax FormatBetween(ArgumentListSyntax arguments, int previousArgIndex, int formattedArgIndex, int nextArgIndex)
        {
            var previousArgEndsWithNewLine = arguments.OpenParenToken.GetStartLinePosition() == arguments.Arguments[previousArgIndex].GetStartLinePosition();
            var nextArgEndsWithNewLine = arguments.OpenParenToken.GetStartLinePosition() == arguments.Arguments[nextArgIndex].GetStartLinePosition();
            var previousArgCommaTrailingTrivia = previousArgEndsWithNewLine
                ? SyntaxFactory.EndOfLine(Environment.NewLine)
                : new SyntaxTrivia();
            var nextArgCommaTrailingTrivia = nextArgEndsWithNewLine
                ? SyntaxFactory.EndOfLine(Environment.NewLine)
                : new SyntaxTrivia();
            var previousArgument = arguments.Arguments[previousArgIndex];
            var nextArgument = arguments.Arguments[nextArgIndex];

            arguments = arguments
                .ReplaceNode(n => n.Arguments[previousArgIndex], arg => arg.WithoutTrailingTrivia(SyntaxKind.EndOfLineTrivia))
                .ReplaceToken(
                    n => n.Arguments[previousArgIndex].GetNextComma(), 
                    c => c
                        .WithoutTrivia(SyntaxKind.WhitespaceTrivia)
                        .AppendTrailingTrivia(SyntaxFactory.Whitespace(" "), previousArgCommaTrailingTrivia))
                .ReplaceNode(n => n.Arguments[nextArgIndex], arg => arg.WithoutTrailingTrivia(SyntaxKind.EndOfLineTrivia))
                .ReplaceToken(
                    n => n.Arguments[nextArgIndex].GetPreviousComma(), 
                    c => c
                        .WithoutTrivia(SyntaxKind.WhitespaceTrivia)
                        .AppendTrailingTrivia(SyntaxFactory.Whitespace(" "), nextArgCommaTrailingTrivia))
                 .ReplaceNode(n => n.Arguments[formattedArgIndex], n => AlignWith(n, previousArgument))
                 .ReplaceNode(n => n.Arguments[formattedArgIndex], n => AlignWith(n, nextArgument));

            return arguments.Arguments[formattedArgIndex];
        }
    }
}
