using System;
using System.Text;

namespace AnalyzeMe.Tests.Helpers
{
	public static class StringExtensions
	{
		public static StringBuilder AppendWithLine(this StringBuilder sb, string appendedString)
		{
			return sb.Append(appendedString).AppendLine();
		}
	}
}
