using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static class SyntaxNodeExtensions
    {
        public static bool TryGetWorkspace(this SyntaxNode node, out Workspace workspace)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var syntaxTreeContainer = node.SyntaxTree.GetText().Container;
            return Workspace.TryGetWorkspace(syntaxTreeContainer, out workspace);
        }

        public static SyntaxTrivia ExtractWhitespace(this SyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var whitespaceExtractor = new LeadWhitespaceExtractor();

            return whitespaceExtractor.Visit(node);
        }

        public static FileLinePositionSpan GetLinePosition(this SyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var ast = node.SyntaxTree;

            return ast.GetLineSpan(node.Span);
        }

        public static SyntaxTriviaList GetLeadingTriviaOnCurrentLine(this SyntaxNode node)
        {
            var currentLinePosition = node.GetStartLinePosition();

            return node
                .GetLeadingTrivia()
                .Where(x => x.GetEndLinePosition() == currentLinePosition)
                .ToSyntaxTriviaList();
        }

        public static int GetStartLinePosition(this SyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return node.GetLinePosition().StartLinePosition.Line;
        }

        public static int GetEndLinePosition(this SyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return node.GetLinePosition().EndLinePosition.Line;
        }

        public static TNode WithoutEolTrivia<TNode>(this TNode node) where TNode : SyntaxNode
        {
            return node.WithTrailingTrivia(node.GetTrailingTrivia().Where(x => !x.IsKind(SyntaxKind.EndOfLineTrivia)));
        }

        public static SyntaxToken GetAssociatedComma(this ArgumentSyntax argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument));
            }

            var argumentList = argument.Parent as ArgumentListSyntax;

            if (argumentList == null || argumentList.Arguments.Count <= 1)
            {
                return SyntaxFactory.Token(SyntaxKind.None);
            }

            var commaIndex = argumentList
                .Arguments
                .TakeWhile(arg => arg != argument)
                .Count() - 1;   // First argument has no associated comma, so CommaIndex for this argument equal -1

            return argumentList.Arguments.GetSeparator(commaIndex);
        }
    }
}
