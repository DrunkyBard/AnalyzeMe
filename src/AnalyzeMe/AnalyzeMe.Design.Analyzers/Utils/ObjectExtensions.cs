using System;
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

	    public static void WhenHasValueThen<T>(this Optional<T> option, Action<T> valueAction)
	    {
		    if (valueAction == null)
		    {
			    throw new ArgumentNullException(nameof(valueAction));
		    }

		    if (option.HasValue)
		    {
			    valueAction(option.Value);
		    }
	    }
    }
}
