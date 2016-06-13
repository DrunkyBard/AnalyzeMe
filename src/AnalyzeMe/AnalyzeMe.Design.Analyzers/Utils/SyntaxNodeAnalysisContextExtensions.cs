using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzeMe.Design.Analyzers.Utils
{
	public static class SyntaxNodeAnalysisContextExtensions
	{
		public static void ReportDiagnostics(this SyntaxNodeAnalysisContext ctx, IReadOnlyCollection<Diagnostic> diagnostics)
		{
			foreach (var diagnostic in diagnostics)
			{
				ctx.ReportDiagnostic(diagnostic);
			}
		}
	}
}
