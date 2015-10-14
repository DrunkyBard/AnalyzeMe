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

        public static ArgumentSyntax InsertBetween(this ArgumentSyntax insertedArgument, ArgumentSyntax afterArgument, ArgumentSyntax beforeArgument)
        {
			throw new NotImplementedException();
        }
    }
}
