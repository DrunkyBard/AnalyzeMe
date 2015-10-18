using System;
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

            if (argIdx == 0)
            {
                return FormatFirst(argument);
            }

            if (argIdx == argumentList.Arguments.Count - 1)
            {
                return FormatLast(argument);
            }

            var previousArg = argumentList.Arguments[argIdx - 1];
            var nextArg = argumentList.Arguments[argIdx + 1];

            return FormatBetween(previousArg, argument, nextArg);
        }

        private static ArgumentSyntax FormatFirst(ArgumentSyntax formattedArgument)
        {
            throw new NotImplementedException();
        }

        private static ArgumentSyntax FormatLast(ArgumentSyntax formattedArgument)
        {
            throw new NotImplementedException();
        }

        private static ArgumentSyntax FormatBetween(ArgumentSyntax previousArgument, ArgumentSyntax formattedArgument, ArgumentSyntax nextArgument)
        {
            throw new NotImplementedException();
        }
    }
}
