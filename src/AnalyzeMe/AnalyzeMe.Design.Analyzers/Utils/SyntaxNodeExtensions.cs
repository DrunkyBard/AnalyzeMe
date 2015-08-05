using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

            if (!node.HasLeadingTrivia)
            {
                return SyntaxFactory.Whitespace(String.Empty);
            }

            var leadingTriviaLength = node.GetLeadingTrivia().Span.Length;
            var fullWhitespace = string.Concat(Enumerable.Repeat(" ", leadingTriviaLength));

            return SyntaxFactory.Whitespace(fullWhitespace);
        }
    }
}
