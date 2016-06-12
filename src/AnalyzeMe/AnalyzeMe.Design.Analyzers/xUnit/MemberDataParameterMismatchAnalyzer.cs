using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

		private async void AnalyzeMethodWithMemberDataAttribute(SyntaxNodeAnalysisContext ctx)
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

		        if (memberNameParameter == null || memberNameParameter.NameColon != null && memberNameParameter.NameColon.ToString() != "memberName")
				{
					continue;
				}

				var memberNameVisitor = new MemberNameExpressionVisitor();
			    var fixtureMethodName = memberNameParameter.Expression.Accept(memberNameVisitor);

				var memberTypeParameter = 
                    memberDataAttribute
		                .Item1
		                .ArgumentList
		                .Arguments
		                .FirstOrDefault(x => x.NameEquals != null && 
										x.NameEquals.Name.ToString().Equals("MemberType", StringComparison.OrdinalIgnoreCase) && 
										x.Expression != null &&
										x.Expression.As<TypeOfExpressionSyntax>().HasValue);

				Workspace ws;
			    ctx.Node.TryGetWorkspace(out ws);

			    var testFixtureClass = memberTypeParameter == null
				    ? (INamedTypeSymbol)ctx.SemanticModel.GetDeclaredSymbol((MethodDeclarationSyntax) ctx.Node).ReceiverType
				    : await FindClassDeclaration(ws, ((TypeOfExpressionSyntax)memberTypeParameter.Expression).Type.ToFullString(), ctx.CancellationToken);

			    if (testFixtureClass == null)
			    {
				    return;
			    }

			    var g = testFixtureClass
				    .DeclaringSyntaxReferences
					.SelectMany(syntaxRef => syntaxRef
									.GetSyntax(ctx.CancellationToken)
									.As<ClassDeclarationSyntax>()
									.Value
									.Members
									.Select(m => m.As<MethodDeclarationSyntax>()))
					.Where(methodDeclarationOpt => methodDeclarationOpt.HasValue && 
												   methodDeclarationOpt.Value.Identifier.ToFullString() == fixtureMethodName);
		    }

		}

		private async Task<INamedTypeSymbol> FindClassDeclaration(Workspace workspace, string name, CancellationToken ct)
		{
			return await workspace
				.CurrentSolution
				.Projects
				.Select(async p => await SymbolFinder.FindDeclarationsAsync(p, name, false, ct))
				.WhenAll()
				.ContinueWith(t => t
								.Result
								.SelectMany(x => x)
								.FirstOrDefault()
								.As<INamedTypeSymbol>()
								.Value, ct);
		}

		private class MemberNameExpressionVisitor : CSharpSyntaxVisitor<string>
		{
			public override string VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				var nameofIdentifier = node.Expression.As<IdentifierNameSyntax>();
				var correctNameofInvocation =
					nameofIdentifier.HasValue &&
					nameofIdentifier.Value.ToFullString() == "nameof" &&
					node.ArgumentList.Arguments.Count == 1;

				if (!correctNameofInvocation)
				{
					return string.Empty;
				}

				var callingMember = node.ArgumentList.Arguments.Single().Expression;

				Debug.Assert(callingMember is MemberAccessExpressionSyntax || callingMember is IdentifierNameSyntax);

				var memberAccessSyntax = callingMember as MemberAccessExpressionSyntax;

				if (memberAccessSyntax != null)
				{
					return memberAccessSyntax.Name.ToFullString();
				}

				return ((IdentifierNameSyntax) callingMember).Identifier.ToFullString();
			}

			public override string VisitLiteralExpression(LiteralExpressionSyntax node)
			{
				return node.Token.ValueText;
			}
		}
	}
}
