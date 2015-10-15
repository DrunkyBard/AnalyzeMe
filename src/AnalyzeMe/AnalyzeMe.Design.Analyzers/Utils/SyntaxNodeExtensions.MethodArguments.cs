using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static partial class SyntaxNodeExtensions
    {
        public static ArgumentSyntax InsertAfter(this ArgumentSyntax insertedArgument, ArgumentSyntax afterArgument)
        {
            return InsertBetween(insertedArgument, afterArgument, null);
        }

        public static ArgumentSyntax InsertBefore(this ArgumentSyntax insertedArgument, ArgumentSyntax beforeArgument)
        {
            return InsertBetween(insertedArgument, null, beforeArgument);
        }

        private static ArgumentSyntax InsertBetween(ArgumentSyntax insertedArgument, ArgumentSyntax afterArgument, ArgumentSyntax beforeArgument)
        {
            if (insertedArgument == null)
            {
                throw new ArgumentNullException(nameof(insertedArgument));
            }

            if (insertedArgument.NameColon == null && afterArgument?.NameColon != null)
            {
                throw new ArgumentException("Argument with name colon cannot be inserted after argument without name colon.");
            }

            if (insertedArgument.NameColon != null && beforeArgument?.NameColon == null)
            {
                throw new ArgumentException("Argument without name colon cannot be inserted before argument with name colon.");
            }

            throw new NotImplementedException();
        }
    }
}
