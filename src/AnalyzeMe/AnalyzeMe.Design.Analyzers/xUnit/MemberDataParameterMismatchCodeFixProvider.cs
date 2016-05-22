using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace AnalyzeMe.Design.Analyzers.xUnit
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MemberDataParameterMismatchCodeFixProvider)), Shared]
	public sealed class MemberDataParameterMismatchCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MemberDataParameterMismatchAnalyzer.MemberDataParameterMismatchDiagnosticId);

		public override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			return null;
		}
	}
}
