using System;
using Microsoft.CodeAnalysis;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static class _
    {
        public static TOutput Match<TInput, TOutput>(
            Optional<TInput> match,
            Func<TInput, TOutput> some,
            Func<TOutput> none)
        {
            if (match.HasValue)
            {
                return some(match.Value);
            }

            return none();
        }
    }
}
