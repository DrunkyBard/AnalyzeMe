using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    internal sealed class LeadWhitespaceExtractor : CSharpSyntaxVisitor<SyntaxTrivia>
    {
        public override SyntaxTrivia VisitIdentifierName(IdentifierNameSyntax node)
        {
            var argumentListOption = node.Parent.Parent.As<ArgumentListSyntax>();
            var argumentSyntax = node.Parent.As<ArgumentSyntax>();

            if (!argumentListOption.HasValue || !argumentSyntax.HasValue)
            {
                throw new ArgumentException("Identifier should be part of method arguments.");
            }

            var argumentList = argumentListOption.Value;
            var identifierIsFirstArgument = argumentList.Arguments.First().Span == node.Span;

            if (identifierIsFirstArgument)
            {
                var identifierAndOpenParenInOneLine = argumentList.OpenParenToken.GetStartLinePosition() == node.GetStartLinePosition();

                if (identifierAndOpenParenInOneLine)
                {
                    return BuildWhitespace(1);
                }
            }

            return BuildWhitespace(argumentSyntax.Value.GetLeadingTriviaOnCurrentLine().Span.Length);
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
                var lArgAndOpenParenInOneLine = lambda.GetStartLinePosition() == argumentList.OpenParenToken.GetStartLinePosition();

                if (labmdaInOneLine && lArgAndOpenParenInOneLine)
                {
                    return BuildWhitespace(1);
                }

                if (lArgAndOpenParenInOneLine)
                {
                    var lamdaBodyOption = lambda.Body.As<BlockSyntax>();

                    return lamdaBodyOption.HasValue
                        ? BuildWhitespace(lamdaBodyOption.Value.CloseBraceToken.GetLeadingTriviaOnCurrentLine().Span.Length)
                        : BuildWhitespace(lambda.Body.GetLeadingTriviaOnCurrentLine().Span.Length);
                }

                return BuildWhitespace(argumentSyntax.Value.GetLeadingTriviaOnCurrentLine().Span.Length);
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
