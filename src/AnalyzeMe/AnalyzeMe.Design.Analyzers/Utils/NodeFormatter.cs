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
                return FormatLast(argument);
            }

            var previousArg = argumentList.Arguments[argIdx - 1];
            var nextArg = argumentList.Arguments[argIdx + 1];

            return FormatBetween(previousArg, argument, nextArg);
        }

        private static ArgumentSyntax FormatFirst(ArgumentListSyntax arguments, ArgumentSyntax formattedArgument)
        {
            var openBraceAndFirstArgumentOnOneLine = arguments.OpenParenToken.GetStartLinePosition() == formattedArgument.GetStartLinePosition();
            var manyArguments = arguments.Arguments.Count > 1;

            if (manyArguments)
            {
                var secondArgumentStartWithNewLine = arguments.OpenParenToken.GetStartLinePosition() != arguments.Arguments[1].GetStartLinePosition();

                if (secondArgumentStartWithNewLine)
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
                        var tt = arguments.Arguments[1].GetAssociatedComma().TrailingTrivia;
                        var eolComma = arguments.Arguments[1]
                            .GetAssociatedComma()
                            .WithTrailingTrivia(
                                SyntaxTriviaList.Create(SyntaxFactory.EndOfLine(Environment.NewLine)).Union(tt));
                        arguments = arguments
                            .ReplaceNode(n => n.Arguments[0], formattedArgument)
                            .ReplaceToken(n => n.Arguments[1].GetAssociatedComma(), eolComma);
                    }
                }
                else
                {
                    formattedArgument = formattedArgument.WithoutFirstLeadingTrivia(SyntaxKind.WhitespaceTrivia);
                    arguments = arguments
                        .ReplaceNode(n => n.Arguments[0], formattedArgument)
                        .ReplaceToken(
                            n => n.OpenParenToken,
                            arguments.OpenParenToken.WithoutFirstTrailingTrivia(SyntaxKind.WhitespaceTrivia))
                        .ReplaceNode(
                            n => n.Arguments[1],
                            arguments.Arguments[1].WithoutFirstLeadingTrivia(SyntaxKind.WhitespaceTrivia));
                }
            }
            else
            {
                if (openBraceAndFirstArgumentOnOneLine)
                {
                    formattedArgument = formattedArgument.WithoutTrivia(SyntaxKind.WhitespaceTrivia);
                    arguments = arguments.ReplaceNode(arguments.Arguments[0], formattedArgument);
                }
            }

            return arguments.Arguments[0];
        }

        private static ArgumentSyntax FormatLast(ArgumentSyntax formattedArgument)
        {
            throw new NotImplementedException();
        }

        private static ArgumentSyntax FormatBetween(ArgumentSyntax previousArgument, ArgumentSyntax formattedArgument, ArgumentSyntax nextArgument)
        {
            throw new NotImplementedException();
        }
    }
}
