using System;
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

        public static SyntaxToken WithoutTrailingTrivia(this SyntaxToken token)
        {
            return token.WithTrailingTrivia();
        }


        public static SyntaxToken WithoutTrailingTrivia(this SyntaxToken token, params SyntaxKind[] excludeTriviaKinds)
        {
            return token
                .WithTrailingTrivia(
                    token.TrailingTrivia
                        .Where(t => !excludeTriviaKinds.Contains(t.Kind()))
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

        public static SyntaxToken AppendTrailingTrivia(this SyntaxToken token, params SyntaxTrivia[] trailingTrivias)
        {
            try
            {
                //var q = token.TrailingTrivia.Union(trailingTrivias);
                //var g = q.ToList();
                var a = token.WithTrailingTrivia(token.TrailingTrivia.Union(trailingTrivias)); //TODO: NRE this
            }
            catch (Exception e)
            {
                var a = 1;
                throw;
            }
            return token.WithTrailingTrivia(token.TrailingTrivia.Union(trailingTrivias));
        }

        public static SyntaxToken AddToTheTopTrailingTrivia(this SyntaxToken token, params SyntaxTrivia[] trailingTrivias)
        {
            return token.WithTrailingTrivia(trailingTrivias.Union(token.TrailingTrivia));
        }

        public static SyntaxToken ReplaceTrivia(this SyntaxToken token, Func<SyntaxToken, SyntaxTrivia> replacementTriviaSelector, SyntaxTrivia newTrivia)
        {
            if (newTrivia.IsKind(SyntaxKind.None))
            {
                return token;
            }

            return token.ReplaceTrivia(replacementTriviaSelector(token), newTrivia);
        }
    }
}
