using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    internal sealed class WhitespaceExtractor : CSharpSyntaxVisitor<SyntaxTrivia>
    {
        public override SyntaxTrivia VisitArgument(ArgumentSyntax node)
        {
            var indentCount = node.HasLeadingTrivia
                ? node.GetLeadingTrivia().Reverse().TakeWhile(x => !x.IsKind(SyntaxKind.EndOfLineTrivia)).ToSyntaxTriviaList().Span.Length
                : node.GetAssociatedComma().TrailingTrivia.Span.Length;

            return BuildWhitespace(indentCount);
        }

        private SyntaxTrivia BuildWhitespace(int indentCount)
        {
            var fullWhitespace = string.Concat(Enumerable.Repeat(" ", indentCount));

            return SyntaxFactory.Whitespace(fullWhitespace);
        }
    }
}
