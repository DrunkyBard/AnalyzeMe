using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AnalyzeMe.Design.Analyzers.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;

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
                .Select(attrSyntax => new Tuple<AttributeSyntax, SymbolInfo>(attrSyntax, ctx.SemanticModel.GetSymbolInfo(attrSyntax, ctx.CancellationToken)))
                .Where(x => x.Item2.Symbol != null && x.Item2.Symbol.ContainingType.ToString().Equals("Xunit.MemberDataAttribute"))
                .ToArray();

			if (!memberDataAttributes.Any())
			{
				return;
			}

		    foreach (var memberDataAttribute in memberDataAttributes)
		    {
		        var memberNameParameter =
		            memberDataAttribute
		                .Item1
		                .ArgumentList
		                .Arguments
		                .FirstOrDefault();

		        //if (memberNameParameter == null || memberNameParameter.NameEquals != null || memberNameParameter.NameColon.ToString() != "memberName")
		        //{
		        //    continue;
		        //}

		        var memberTypeParameter = 
                    memberDataAttribute
		                .Item1
		                .ArgumentList
		                .Arguments
		                .FirstOrDefault(x => x.NameEquals != null && x.NameEquals.Name.ToString() == "MemberType" && x.Expression != null);

			    Workspace ws;
			    ctx.Node.TryGetWorkspace(out ws);

			    var testFixtureClass = memberTypeParameter == null
				    ? ctx.SemanticModel.GetDeclaredSymbol((MethodDeclarationSyntax) ctx.Node)
				    : FindClassDeclaration(ws, memberTypeParameter.ToFullString());
		    }

		}

		private ISymbol FindClassDeclaration(Workspace workspace, string name)
		{
			var a = workspace
				.CurrentSolution
				.Projects
				.Select(p => SymbolFinder.FindDeclarationsAsync(p, name, false))
				.ToArray();
			var res = Task
				.WhenAll(a)
				.ContinueWith(t => t.Result)
				.Result
				.SelectMany(x => x)
				.ToArray();

			return null;
		}
	}
}
