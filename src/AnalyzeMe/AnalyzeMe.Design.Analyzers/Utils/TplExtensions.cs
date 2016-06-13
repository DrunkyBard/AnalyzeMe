using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnalyzeMe.Design.Analyzers.Utils
{
	public static class TplExtensions
	{
		public static async Task<TOutput[]> WhenAll<TOutput>(this IEnumerable<Task<TOutput>> taskArray)
		{
			return await Task.WhenAll(taskArray).ConfigureAwait(false);
		}
	}
}
