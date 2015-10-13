using Microsoft.CodeAnalysis;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static class ObjectExtensions
    {
        public static Optional<TOutput> As<TOutput>(this object source) 
            where TOutput : class
        {
            var destination = source as TOutput;

            return destination == null
                ? new Optional<TOutput>()
                : new Optional<TOutput>(destination);
        }
    }
}
