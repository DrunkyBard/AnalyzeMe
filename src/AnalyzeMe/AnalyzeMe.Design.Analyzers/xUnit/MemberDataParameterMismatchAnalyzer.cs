using System.Collections.Immutable;
using System.Linq;
using AnalyzeMe.Design.Analyzers.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzeMe.Design.Analyzers.xUnit
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class MemberDataParameterMismatchAnalyzer : DiagnosticAnalyzer
	{
		public const string MemberDataParameterMismatchDiagnosticId = "MemberDataParameterMismatch";
		internal static readonly LocalizableString MemberDataParameterMismatchTitle = "Class can be marked as sealed.";
		internal static readonly LocalizableString MemberDataParameterMismatchMessageFormat = "Class can be marked as sealed.";
		internal const string MemberDataParameterMismatchCategory = "Usage";
		internal static readonly DiagnosticDescriptor MemberDataParameterMismatchRule = new DiagnosticDescriptor(
			MemberDataParameterMismatchDiagnosticId,
			MemberDataParameterMismatchTitle,
			MemberDataParameterMismatchMessageFormat,
			MemberDataParameterMismatchCategory,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true);
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MemberDataParameterMismatchRule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeMethodWithMemberDataAttribute, SyntaxKind.MethodDeclaration);
		}

		private void AnalyzeMethodWithMemberDataAttribute(SyntaxNodeAnalysisContext ctx)
		{
			var memberDataAttributes = ctx.Node
				.As<MethodDeclarationSyntax>()
				.Value
				.AttributeLists
				.SelectMany(x => x.Attributes)
				.Where(x => x.Name.ToString() == "MemberData")
				.ToArray();

			var a = memberDataAttributes[0];
			var c = ctx.SemanticModel.GetDeclaredSymbol(a, ctx.CancellationToken);

			if (!memberDataAttributes.Any())
			{
				return;
			}
		}
	}
}
