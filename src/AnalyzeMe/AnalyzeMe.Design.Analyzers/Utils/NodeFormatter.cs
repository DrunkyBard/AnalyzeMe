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
                .WithoutTrailingTrivia(SyntaxKind.WhitespaceTrivia)
                .AddToTheTopTrailingTrivia(SyntaxFactory.Whitespace(" "))
                .ReplaceTrivia(t => t.TrailingTrivia.LastOrDefault(x => x.IsKind(SyntaxKind.EndOfLineTrivia)), nextCommaLastTrailingTrivia);

            arguments = arguments
                .ReplaceToken(
                    arguments.OpenParenToken, 
                    arguments.OpenParenToken
                        .WithoutTrailingTrivia(SyntaxKind.EndOfLineTrivia, SyntaxKind.WhitespaceTrivia)
                        .AppendTrailingTrivia(openParenTrailingTrivia))
                .ReplaceNode(n => n.Arguments[0], arguments.Arguments[0].WithoutLeadingTrivia(SyntaxKind.WhitespaceTrivia))
                .ReplaceNode(n => n.Arguments[0], arguments.Arguments[0].WithoutTrailingTrivia(SyntaxKind.WhitespaceTrivia, SyntaxKind.EndOfLineTrivia))
                .ReplaceToken(n => n.Arguments[0].GetNextComma(), formattedComma)                
                .ReplaceNode(n => n.Arguments[0], n => AlignWith(n, n.TryGetNextArgument()));

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
            throw new NotImplementedException();
        }

        private static ArgumentSyntax FormatBetween(ArgumentSyntax previousArgument, ArgumentSyntax formattedArgument, ArgumentSyntax nextArgument)
        {
            throw new NotImplementedException();
        }
    }
}
