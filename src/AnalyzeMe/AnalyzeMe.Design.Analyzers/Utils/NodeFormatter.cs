﻿using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static System.String;

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
                ? SyntaxFactory.Whitespace(Empty)
                : (!argumentStartsWithNewLine
                    ? SyntaxFactory.Whitespace(Empty)
                    : SyntaxFactory.CarriageReturnLineFeed);
            var openParenTrailingTrivia = argumentStartsWithNewLine 
                ? SyntaxFactory.CarriageReturnLineFeed
                : SyntaxFactory.Whitespace(Empty);
            var formattedComma = nextComma
                .WithoutLeadingTrivia(SyntaxKind.WhitespaceTrivia)
                .WithoutLastTrailingTrivia(SyntaxKind.WhitespaceTrivia)
                .AddToTheTopTrailingTrivia(SyntaxFactory.Space)
                .ReplaceTrivia(t => t.TrailingTrivia.LastOrDefault(x => x.IsKind(SyntaxKind.EndOfLineTrivia)), nextCommaLastTrailingTrivia);

            arguments = arguments
                .ReplaceToken(
                    arguments.OpenParenToken, 
                    arguments.OpenParenToken
                        .WithoutLastTrailingTrivia(SyntaxKind.EndOfLineTrivia).WithoutLastTrailingTrivia(SyntaxKind.WhitespaceTrivia)
                        .AppendTrailingTrivia(openParenTrailingTrivia))
                .ReplaceNode(
                        n => n.Arguments[0], 
                        arguments.Arguments[0]
                            .WithoutTrivia(SyntaxKind.WhitespaceTrivia)
                            .WithoutLastTrailingTrivia(SyntaxKind.EndOfLineTrivia))
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

            var leadingWhitespace = Concat(Enumerable.Repeat(" ", secondArgumentWhitespaceLength - firstArgumentLeadingTriviaLength));

            return argument.AddLeadingTrivia(SyntaxFactory.Whitespace(leadingWhitespace));
        }

        private static ArgumentSyntax FormatLast(ArgumentListSyntax arguments, ArgumentSyntax formattedArgument)
        {
            var argumentsCount = arguments.Arguments.Count;
            var previousArgument = arguments.Arguments[argumentsCount - 2];
            var previousArgumentStartsWithNewLine = arguments.OpenParenToken.GetStartLinePosition() != previousArgument.GetStartLinePosition();
            var lastCommaTrailingTrivia = previousArgumentStartsWithNewLine
                ? SyntaxFactory.CarriageReturnLineFeed
                : SyntaxFactory.Whitespace(Empty);
            var lastOriginComma = formattedArgument.GetPreviousComma();
            var lastFormattedComma = lastOriginComma
                .WithoutTrivia(SyntaxKind.WhitespaceTrivia)
                .WithoutLastTrailingTrivia(SyntaxKind.EndOfLineTrivia)
                .AddToTheTopTrailingTrivia(SyntaxFactory.Space)
                .AppendTrailingTrivia(lastCommaTrailingTrivia);

            arguments = arguments
                .ReplaceNode(previousArgument, previousArgument.WithoutLastTrailingTrivia(SyntaxKind.WhitespaceTrivia, SyntaxKind.EndOfLineTrivia))
                .ReplaceToken(n => n.Arguments.Last().GetPreviousComma(), lastFormattedComma)
                .ReplaceNode(n => n.Arguments.Last(), arg => AlignWith(arg, previousArgument).WithoutLastTrailingTrivia(SyntaxKind.WhitespaceTrivia, SyntaxKind.EndOfLineTrivia))
                .ReplaceToken(n => n.CloseParenToken, t => t.WithoutLeadingTrivia(SyntaxKind.WhitespaceTrivia).WithoutLeadingTrivia(SyntaxKind.EndOfLineTrivia));

            return arguments.Arguments.Last();
        }

        private static ArgumentSyntax FormatBetween(ArgumentListSyntax arguments, int previousArgIndex, int formattedArgIndex, int nextArgIndex)
        {
            var startWithNewLine = arguments.Arguments[previousArgIndex].GetStartLinePosition() != arguments.Arguments[nextArgIndex].GetStartLinePosition();
            var previousArgEndsWithNewLine = arguments.OpenParenToken.GetStartLinePosition() != arguments.Arguments[previousArgIndex].GetStartLinePosition();
            var nextArgEndsWithNewLine = arguments.OpenParenToken.GetStartLinePosition() != arguments.Arguments[nextArgIndex].GetStartLinePosition();
            //var previousArgCommaTrailingTrivia = previousArgEndsWithNewLine
            //    ? SyntaxFactory.CarriageReturnLineFeed
            //    : SyntaxFactory.Whitespace(Empty);
            //var nextArgCommaTrailingTrivia = nextArgEndsWithNewLine
            //    ? SyntaxFactory.CarriageReturnLineFeed
            //    : SyntaxFactory.Whitespace(Empty);

            var previousArgCommaTrailingTrivia = startWithNewLine
                ? SyntaxFactory.CarriageReturnLineFeed
                : SyntaxFactory.Whitespace(Empty);
            var nextArgCommaTrailingTrivia = startWithNewLine
                ? SyntaxFactory.CarriageReturnLineFeed
                : SyntaxFactory.Whitespace(Empty);
            var previousArgument = arguments.Arguments[previousArgIndex];
            var nextArgument = arguments.Arguments[nextArgIndex];

            arguments = arguments
                .ReplaceNode(n => n.Arguments[previousArgIndex], arg => arg.WithoutLastTrailingTrivia(SyntaxKind.EndOfLineTrivia))
                .ReplaceToken(
                    n => n.Arguments[previousArgIndex].GetNextComma(), 
                    c => c
                        .WithoutTrivia(SyntaxKind.WhitespaceTrivia)
                        .WithoutLastTrailingTrivia(SyntaxKind.EndOfLineTrivia)
                        .AddToTheTopTrailingTrivia(SyntaxFactory.Space)
                        .AppendTrailingTrivia(previousArgCommaTrailingTrivia))
                .ReplaceNode(n => n.Arguments[nextArgIndex], arg => arg.WithoutLastTrailingTrivia(SyntaxKind.EndOfLineTrivia))
                .ReplaceToken(
                    n => n.Arguments[nextArgIndex].GetPreviousComma(), 
                    c => c
                        .WithoutTrivia(SyntaxKind.WhitespaceTrivia)
                        .WithoutLastTrailingTrivia(SyntaxKind.EndOfLineTrivia)
                        .AddToTheTopTrailingTrivia(SyntaxFactory.Space)
                        .AppendTrailingTrivia(nextArgCommaTrailingTrivia))
                 //.ReplaceNode(n => n.Arguments[formattedArgIndex], n => AlignWith(n, previousArgument))
                 .ReplaceNode(n => n.Arguments[formattedArgIndex], (argList, arg) => AlignWith(arg, argList.Arguments[nextArgIndex]));

            return arguments.Arguments[formattedArgIndex];
        }
    }
}
