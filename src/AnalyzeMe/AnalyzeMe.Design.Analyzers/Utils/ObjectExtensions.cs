using Microsoft.CodeAnalysis;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static class ObjectExtensions
    {
        public static Optional<TOutput> As<TOutput>(this object source) 
            where TOutput : class
        {
            return new Optional<TOutput>(source as TOutput);
        }
    }
}
