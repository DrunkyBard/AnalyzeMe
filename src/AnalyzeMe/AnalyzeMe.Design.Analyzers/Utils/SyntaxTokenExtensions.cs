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
                .Where(x => x.GetStartLinePosition() == x.GetEndLinePosition() && x.GetEndLinePosition() == currentLinePosition)
                .ToSyntaxTriviaList();
        }

        public static SyntaxToken WithoutLeadingTrivia(this SyntaxToken token, SyntaxKind excludeTriviaKind)
        {
            return token
                .WithLeadingTrivia(
                    token.LeadingTrivia
                        .Where(t => !t.IsKind(excludeTriviaKind))
                );
        }

        public static SyntaxToken WithoutLastLeadingTrivia(this SyntaxToken token, SyntaxKind excludeTriviaKind)
        {
            return token
                .WithLeadingTrivia(
                    token.LeadingTrivia
                        .TakeWhile(t => !t.IsKind(excludeTriviaKind))
                );
        }

        public static SyntaxToken WithoutFirstLeadingTrivia(this SyntaxToken token, SyntaxKind excludeTriviaKind)
        {
            return token
                .WithLeadingTrivia(
                    token.LeadingTrivia
                        .SkipWhile(t => t.IsKind(excludeTriviaKind))
                );
        }

        public static SyntaxToken WithoutTrailingTrivia(this SyntaxToken token, SyntaxKind excludeTriviaKind)
        {
            return token
                .WithTrailingTrivia(
                    token.TrailingTrivia
                        .Where(t => !t.IsKind(excludeTriviaKind))
                );
        }

        public static SyntaxToken WithoutLastTrailingTrivia(this SyntaxToken token, SyntaxKind excludeTriviaKind)
        {
            // TODO: Check this
            return token
                .WithTrailingTrivia(
                    token.TrailingTrivia
                        .Reverse()
                        .SkipWhile(t => t.IsKind(excludeTriviaKind))
                        .Reverse()
                );
        }

        public static SyntaxToken WithoutFirstTrailingTrivia(this SyntaxToken token, SyntaxKind excludeTriviaKind)
        {
            return token
                .WithTrailingTrivia(
                    token.TrailingTrivia
                        .SkipWhile(t => t.IsKind(excludeTriviaKind))
                );
        }

        public static SyntaxToken WithoutTrivia(this SyntaxToken token, SyntaxKind excludeTriviaKind)
        {
            return token
                .WithoutLeadingTrivia(excludeTriviaKind)
                .WithoutTrailingTrivia(excludeTriviaKind);
        }
    }
}
