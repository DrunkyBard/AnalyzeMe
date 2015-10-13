using Microsoft.CodeAnalysis;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static class SyntaxTriviaExtensions
    {
        public static FileLinePositionSpan GetLinePosition(this SyntaxTrivia trivia)
        {
            return trivia.SyntaxTree.GetLineSpan(trivia.Span);
        }

        public static int GetStartLinePosition(this SyntaxTrivia trivia)
        {
            return trivia.GetLinePosition().StartLinePosition.Line;
        }

        public static int GetEndLinePosition(this SyntaxTrivia trivia)
        {
            return trivia.GetLinePosition().EndLinePosition.Line;
        }
    }
}
