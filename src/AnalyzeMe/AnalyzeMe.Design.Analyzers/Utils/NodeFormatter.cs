using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static class NodeFormatter
    {
        public static ArgumentSyntax Format(this ArgumentSyntax argument)
        {
            var argumentListOption = argument.Parent.As<ArgumentListSyntax>();

            if (!argumentListOption.HasValue)
            {
                return argument;
            }

            var argumentList = argumentListOption.Value;
            var argIdx = argumentList.Arguments.IndexOf(argument);

            if (argIdx == argumentList.Arguments.Count - 1)
            {
                
            }

            var previousArg = argumentList.Arguments[argIdx - 1];
            var nextArg = argumentList.Arguments[argIdx + 1];

            return null;
        }
    }
}
