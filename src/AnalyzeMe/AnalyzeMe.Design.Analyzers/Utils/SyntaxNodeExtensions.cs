using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static partial class SyntaxNodeExtensions
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

            var whitespaceExtractor = new ArgumentLeadWhitespaceExtractor();

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

            var argumentListOption = argument.Parent.As<ArgumentListSyntax>();

            if (!argumentListOption.HasValue || argumentListOption.Value.Arguments.Count <= 1)
            {
                return SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.None, SyntaxTriviaList.Empty);
                return SyntaxFactory.Token(SyntaxKind.None);
            }

            var argumentList = argumentListOption.Value;
            var commaIndex = argumentList
                .Arguments
                .TakeWhile(arg => arg.Span != argument.Span)
                .Count() - 1;   // First argument has no associated comma, so CommaIndex for this argument equal -1

            return argumentList.Arguments.GetSeparator(commaIndex);
        }

        public static TNode WithoutLeadingTrivia<TNode>(this TNode node, SyntaxKind excludeTriviaKind) where TNode : SyntaxNode
        {
            return node
                .WithLeadingTrivia(
                    node.GetLeadingTrivia()
                        .Where(t => !t.IsKind(excludeTriviaKind))
                );
        }

        public static TNode WithoutLastLeadingTrivia<TNode>(this TNode node, SyntaxKind excludeTriviaKind) where TNode : SyntaxNode
        {
            return node
                .WithLeadingTrivia(
                    node.GetLeadingTrivia()
                        .TakeWhile(t => !t.IsKind(excludeTriviaKind))
                );
        }

        public static TNode WithoutFirstLeadingTrivia<TNode>(this TNode node, SyntaxKind excludeTriviaKind) where TNode : SyntaxNode
        {
            return node
                .WithLeadingTrivia(
                    node.GetLeadingTrivia()
                        .SkipWhile(t => t.IsKind(excludeTriviaKind))
                );
        }

        public static TNode WithoutTrailingTrivia<TNode>(this TNode node, SyntaxKind excludeTriviaKind) where TNode : SyntaxNode
        {
            return node
                .WithTrailingTrivia(
                    node.GetTrailingTrivia()
                        .Where(t => !t.IsKind(excludeTriviaKind))
                );
        }

        public static TNode WithoutLastTrailingTrivia<TNode>(this TNode node, SyntaxKind excludeTriviaKind) where TNode : SyntaxNode
        {
            // TODO: Check this
            return node
                .WithTrailingTrivia(
                    node.GetTrailingTrivia()
                        .Reverse()
                        .SkipWhile(t => t.IsKind(excludeTriviaKind))
                        .Reverse()
                );
        }

        public static TNode WithoutFirstTrailingTrivia<TNode>(this TNode node, SyntaxKind excludeTriviaKind) where TNode : SyntaxNode
        {
            return node
                .WithTrailingTrivia(
                    node.GetTrailingTrivia()
                        .SkipWhile(t => t.IsKind(excludeTriviaKind))
                );
        }

        public static TNode WithoutTrivia<TNode>(this TNode node, SyntaxKind excludeTriviaKind) where TNode : SyntaxNode
        {
            return node
                .WithoutLeadingTrivia(excludeTriviaKind)
                .WithoutTrailingTrivia(excludeTriviaKind);
        }

        public static TNode ReplaceNode<TNode>(this TNode node, Func<TNode, SyntaxNode> replacementNodeSelector, SyntaxNode newNode) 
            where TNode : SyntaxNode
        {
            return node.ReplaceNode(replacementNodeSelector(node), newNode);
        }

        public static TNode ReplaceToken<TNode>(this TNode node, Func<TNode, SyntaxToken> replacementTokenSelector, SyntaxToken newToken) 
            where TNode : SyntaxNode
        {
            return node.ReplaceToken(replacementTokenSelector(node), newToken);
        }
    }
}
