using Microsoft.CodeAnalysis;

namespace RoslynAnalyzers.Extensions
{
    public static class OptionalExtension
    {
        public static bool Is<T>(this Optional<object> source)
        {
            return source.HasValue && source.Value is T;
        }
    }
}
