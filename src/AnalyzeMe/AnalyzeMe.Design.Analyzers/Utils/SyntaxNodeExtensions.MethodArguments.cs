using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static partial class SyntaxNodeExtensions
    {
        public static ArgumentSyntax InsertAfter(this ArgumentSyntax insertedArgument, ArgumentSyntax afterArgument)
        {
            var argumentListOption = afterArgument.Parent.As<ArgumentListSyntax>();

            if (!argumentListOption.HasValue)
            {
                throw new ArgumentException("After argument should be part of method arguments");
            }

            var beforeArgument = argumentListOption.Value
                .Arguments
                .SkipWhile(x => x.Span != afterArgument.Span)
                .Skip(1)
                .FirstOrDefault();

            return InsertBetween(argumentListOption.Value, insertedArgument, afterArgument, beforeArgument);
        }

        public static ArgumentSyntax InsertBefore(this ArgumentSyntax insertedArgument, ArgumentSyntax beforeArgument)
        {
            var argumentListOption = beforeArgument.Parent.As<ArgumentListSyntax>();

            if (!argumentListOption.HasValue)
            {
                throw new ArgumentException("Before argument should be part of method arguments");
            }

            return InsertBetween(argumentListOption.Value, insertedArgument, null, beforeArgument);
        }

        private static ArgumentSyntax InsertBetween(ArgumentListSyntax argumentList, ArgumentSyntax insertedArgument, ArgumentSyntax afterArgument, ArgumentSyntax beforeArgument)
        {
            if (insertedArgument == null)
            {
                throw new ArgumentNullException(nameof(insertedArgument));
            }

            if (afterArgument == null && beforeArgument == null)
            {
                throw new ArgumentException("After argument and Before argument cannot be null together.");
            }

            if (insertedArgument.NameColon == null && afterArgument?.NameColon != null)
            {
                throw new ArgumentException("Argument with name colon cannot be inserted after argument without name colon.");
            }

            if (insertedArgument.NameColon != null && beforeArgument?.NameColon == null)
            {
                throw new ArgumentException("Argument without name colon cannot be inserted before argument with name colon.");
            }

            if (beforeArgument == null)
            {
                return InsertLast(argumentList, insertedArgument, afterArgument);
            }

            var argumentsBefore = argumentList.Arguments.TakeWhile(x => x.Span != beforeArgument.Span);
            var argumentsAfter = argumentList.Arguments.SkipWhile(x => x.Span != beforeArgument.Span);
            var comma = beforeArgument.GetAssociatedComma();
            var argumentSeparators = argumentList.Arguments.GetSeparators().ToArray();
            var separatorsBefore = argumentSeparators.TakeWhile(x => x.Span != comma.Span);
            var separatorsAfter = argumentSeparators.SkipWhile(x => x.Span != comma.Span);
            
            var idx = argumentList.Arguments.IndexOf(beforeArgument);
            var whitespace = SyntaxTriviaList.Create(SyntaxFactory.Whitespace(" "));
            var updateArgument = argumentsBefore
                .Union(new[] {insertedArgument})
                .Union(argumentsAfter);
            var updateSeparators = separatorsBefore
                .Union(new[] {SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.CommaToken, whitespace)})
                .Union(separatorsAfter);
            
            return argumentList
                .WithArguments(SyntaxFactory.SeparatedList(updateArgument, updateSeparators))
                .Arguments[idx];
        }

        private static ArgumentSyntax InsertLast(ArgumentListSyntax arguments, ArgumentSyntax insertedArgument, ArgumentSyntax lastArgument)
        {
            return arguments
                .AddArguments(insertedArgument)
                .Arguments
                .Last();

            //var newArguments = argumentList.Arguments.Add();
            //var insertedComma = SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.CommaToken, SyntaxTriviaList.Empty);
            //var newCommas = argumentList.Arguments.GetSeparators().ToList();
            //newCommas.Add(insertedComma);

            //oldArguments.WithArguments(SyntaxFactory.SeparatedList(newArguments, newCommas));
        }
    }
}
