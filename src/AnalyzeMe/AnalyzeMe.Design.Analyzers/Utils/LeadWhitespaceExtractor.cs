using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    internal sealed class LeadWhitespaceExtractor : CSharpSyntaxVisitor<SyntaxTrivia>
    {
        public override SyntaxTrivia VisitArgument(ArgumentSyntax node)
        {
            var indentCount = node.HasLeadingTrivia
                ? node.GetLeadingTriviaOnCurrentLine().Span.Length
                : node.GetAssociatedComma().TrailingTrivia.Span.Length;
            
            return BuildWhitespace(indentCount);
        }

        public override SyntaxTrivia VisitIdentifierName(IdentifierNameSyntax node)
        {
            return base.VisitIdentifierName(node);
        }

        public override SyntaxTrivia VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            var argumentListOption = node.Parent.Parent.As<ArgumentListSyntax>();
            var argumentSyntax = node.Parent.As<ArgumentSyntax>();

            if (!argumentListOption.HasValue || !argumentSyntax.HasValue)
            {
                throw new ArgumentException("Lambda expression should be part of method arguments.");
            }

            var argumentList = argumentListOption.Value;
            var lambdaIsFirstArgument = argumentList.Arguments.First().Span == node.Span;
            var labmdaInOneLine = node.GetStartLinePosition() == node.GetEndLinePosition();

            if (lambdaIsFirstArgument)
            {
                var lArgAndOpenParenInOneLine = node.GetStartLinePosition() == argumentList.OpenParenToken.GetStartLinePosition();

                if (labmdaInOneLine && lArgAndOpenParenInOneLine)
                {
                    return BuildWhitespace(1);
                }

                if (lArgAndOpenParenInOneLine)
                {
                    var lamdaBodyOption = node.Body.As<BlockSyntax>();

                    var lPos = lamdaBodyOption.Value.CloseBraceToken.GetLinePosition();

                    return lamdaBodyOption.HasValue
                        ? BuildWhitespace(lamdaBodyOption.Value.CloseBraceToken.GetLeadingTriviaOnCurrentLine().Span.Length)
                        : BuildWhitespace(node.Body.GetLeadingTriviaOnCurrentLine().Span.Length);
                }

                return BuildWhitespace(argumentSyntax.Value.GetLeadingTriviaOnCurrentLine().Span.Length);
            }

            return BuildWhitespace(argumentSyntax.Value.GetLeadingTriviaOnCurrentLine().Span.Length);
        }

        public override SyntaxTrivia VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            var argumentListOption = node.Parent.Parent.As<ArgumentListSyntax>();
            var argumentSyntax = node.Parent.As<ArgumentSyntax>();

            if (!argumentListOption.HasValue || !argumentSyntax.HasValue)
            {
                throw new ArgumentException("Lambda expression should be part of method arguments.");
            }

            var argumentList = argumentListOption.Value;
            var lambdaIsFirstArgument = argumentList.Arguments.First().Expression.Span == node.Span;
            var labmdaInOneLine = node.GetStartLinePosition() == node.GetEndLinePosition();

            if (lambdaIsFirstArgument)
            {
                var lArgAndOpenParenInOneLine = node.GetStartLinePosition() == argumentList.OpenParenToken.GetStartLinePosition();

                if (labmdaInOneLine && lArgAndOpenParenInOneLine)
                {
                    return BuildWhitespace(1);
                }

                if (lArgAndOpenParenInOneLine)
                {
                    var lamdaBodyOption = node.Body.As<BlockSyntax>();

                    var lPos = lamdaBodyOption.Value.CloseBraceToken.GetLinePosition();

                    return lamdaBodyOption.HasValue
                        ? BuildWhitespace(lamdaBodyOption.Value.CloseBraceToken.GetLeadingTriviaOnCurrentLine().Span.Length)
                        : BuildWhitespace(node.Body.GetLeadingTriviaOnCurrentLine().Span.Length);
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
