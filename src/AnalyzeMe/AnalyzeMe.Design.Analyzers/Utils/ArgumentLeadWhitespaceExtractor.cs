using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    internal sealed class ArgumentLeadWhitespaceExtractor : CSharpSyntaxVisitor<SyntaxTrivia>
    {
        public override SyntaxTrivia VisitArgument(ArgumentSyntax node)
        {
            return Visit(node.Expression);
        }

        public override SyntaxTrivia VisitIdentifierName(IdentifierNameSyntax node)
        {
            var argumentListOption = node.Parent.Parent.As<ArgumentListSyntax>();
            var argumentSyntaxOption = node.Parent.As<ArgumentSyntax>();

            if (!argumentListOption.HasValue || !argumentSyntaxOption.HasValue)
            {
                throw new ArgumentException("Identifier should be part of method arguments.");
            }

            var argumentList = argumentListOption.Value;
            var argumentSyntax = argumentSyntaxOption.Value;
            var identifierIsFirstArgument = argumentList.Arguments.First().Span == argumentSyntax.Span;

            if (identifierIsFirstArgument)
            {
                var identifierAndOpenParenInOneLine = argumentList.OpenParenToken.GetStartLinePosition() == node.GetStartLinePosition();

                if (identifierAndOpenParenInOneLine)
                {
                    var openParenTrailingLength = argumentList.OpenParenToken.TrailingTrivia.Span.Length;

                    return BuildWhitespace(openParenTrailingLength > 0 ? openParenTrailingLength : 1);
                }

                return BuildWhitespace(argumentSyntax.GetLeadingTriviaOnCurrentLine().Span.Length);
            }

            var argumentComma = argumentSyntax.GetAssociatedComma();
            var commaAndArgumentOnOneLine = argumentComma.GetStartLinePosition() == argumentSyntax.GetStartLinePosition();

            if (commaAndArgumentOnOneLine)
            {
                return BuildWhitespace(argumentComma.TrailingTrivia.Span.Length + argumentSyntax.GetLeadingTrivia().Span.Length);
            }

            return BuildWhitespace(argumentSyntax.GetLeadingTriviaOnCurrentLine().Span.Length);
        }

        public override SyntaxTrivia VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            return VisitLambdaExpression(node);
        }

        public override SyntaxTrivia VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            return VisitLambdaExpression(node);
        }

        private SyntaxTrivia VisitLambdaExpression<TLambda>(TLambda lambda) where TLambda : LambdaExpressionSyntax
        {
            var argumentListOption = lambda.Parent.Parent.As<ArgumentListSyntax>();
            var argumentSyntax = lambda.Parent.As<ArgumentSyntax>();

            if (!argumentListOption.HasValue || !argumentSyntax.HasValue)
            {
                throw new ArgumentException("Lambda expression should be part of method arguments.");
            }

            var argumentList = argumentListOption.Value;
            var lambdaIsFirstArgument = argumentList.Arguments.First().Expression.Span == lambda.Span;
            var labmdaInOneLine = lambda.GetStartLinePosition() == lambda.GetEndLinePosition();

            if (lambdaIsFirstArgument)
            {
                var lambdaArgAndOpenParenInOneLine = lambda.GetStartLinePosition() == argumentList.OpenParenToken.GetStartLinePosition();

                if (labmdaInOneLine && lambdaArgAndOpenParenInOneLine)
                {
                    return BuildWhitespace(1);
                }

                if (lambdaArgAndOpenParenInOneLine)
                {
                    var lamdaBodyOption = lambda.Body.As<BlockSyntax>();

                    return lamdaBodyOption.HasValue
                        ? BuildWhitespace(lamdaBodyOption.Value.CloseBraceToken.GetLeadingTriviaOnCurrentLine().Span.Length)
                        : BuildWhitespace(lambda.Body.GetLeadingTriviaOnCurrentLine().Span.Length);
                }

                return BuildWhitespace(argumentSyntax.Value.GetLeadingTriviaOnCurrentLine().Span.Length);
            }

            var argumentComma = argumentSyntax
                .Value
                .GetAssociatedComma();
            var commaAndArgumentOnOneLine = argumentComma.GetStartLinePosition() == argumentSyntax.Value.GetStartLinePosition();

            if (commaAndArgumentOnOneLine)
            {
                return BuildWhitespace(argumentComma.TrailingTrivia.Span.Length);
            }

            return BuildWhitespace(argumentSyntax.Value.GetLeadingTriviaOnCurrentLine().Span.Length);
        }

        private SyntaxTrivia BuildWhitespace(int indentCount)
        {
            var fullWhitespace = string.Concat(Enumerable.Repeat(" ", indentCount));

            return SyntaxFactory.Whitespace(fullWhitespace);
        }
    }
}
