using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static class SyntaxTokenExtensions
    {
        public static FileLinePositionSpan GetLinePosition(this SyntaxToken token)
        {
            var ast = token.SyntaxTree;

            return ast.GetLineSpan(token.Span);
        }

        public static int GetStartLinePosition(this SyntaxToken token)
        {
            return token.GetLinePosition().StartLinePosition.Line;
        }

        public static int GetEndLinePosition(this SyntaxToken token)
        {
            return token.GetLinePosition().EndLinePosition.Line;
        }

        public static SyntaxTriviaList GetLeadingTriviaOnCurrentLine(this SyntaxToken token)
        {
            var currentLinePosition = token.GetStartLinePosition();

            return token
                .LeadingTrivia
                .Where(x => x.GetEndLinePosition() == currentLinePosition)
                .ToSyntaxTriviaList();
        }
    }
}
