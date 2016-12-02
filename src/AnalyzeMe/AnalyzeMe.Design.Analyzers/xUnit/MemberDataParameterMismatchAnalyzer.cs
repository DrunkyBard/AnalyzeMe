using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
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
			var methodDeclaration = (MethodDeclarationSyntax)ctx.Node;
			var memberDataAttributes = methodDeclaration
				.AttributeLists
				.SelectMany(x => x.Attributes)
                .Select(attrSyntax => new Tuple<AttributeSyntax, SymbolInfo>(attrSyntax, ctx.SemanticModel.GetSymbolInfo(attrSyntax, ctx.CancellationToken)))
                .Where(x => x.Item2.Symbol != null && x.Item2.Symbol.ContainingType.ToString().Equals("Xunit.MemberDataAttribute"))
                .ToArray();

			if (!memberDataAttributes.Any())
			{
				return;
			}

			var testMethodParameterTypes = methodDeclaration
				.ParameterList
				.Parameters
				.Select(param => ctx.SemanticModel.GetDeclaredSymbol(param).Type)
				.ToArray();

		    foreach (var memberDataAttribute in memberDataAttributes)
		    {
		        var memberNameParameter =
		            memberDataAttribute
		                .Item1
		                .ArgumentList
		                .Arguments
		                .FirstOrDefault(x => x.NameColon != null && x.NameColon.ToString().Equals("memberName", StringComparison.Ordinal) || 
											 x.NameColon == null);

		        if (memberNameParameter == null)
				{
					continue;
				}

				var memberNameVisitor = new MemberNameExpressionVisitor();
			    var fixtureMemberName = memberNameParameter.Expression.Accept(memberNameVisitor);

				var memberTypeParameter = 
                    memberDataAttribute
		                .Item1
		                .ArgumentList
		                .Arguments
		                .FirstOrDefault(x => 
										x.NameEquals != null && 
										x.NameEquals.Name.ToString().Equals("MemberType", StringComparison.Ordinal) && 
										x.Expression != null &&
										x.Expression.As<TypeOfExpressionSyntax>().HasValue);

				Workspace ws;
			    ctx.Node.TryGetWorkspace(out ws);
			    var nam =ctx.SemanticModel.GetDeclaredSymbol((MethodDeclarationSyntax) ctx.Node).ReceiverType.Name;


				var testFixtureClass = memberTypeParameter == null
				    ? (INamedTypeSymbol)ctx.SemanticModel.GetDeclaredSymbol((MethodDeclarationSyntax) ctx.Node).ReceiverType
				    : (INamedTypeSymbol)ctx.SemanticModel.GetSymbolInfo(((TypeOfExpressionSyntax)memberTypeParameter.Expression).Type).Symbol;
				    //: FindClassDeclaration(ws, ((TypeOfExpressionSyntax)memberTypeParameter.Expression).Type.ToFullString(), ctx.CancellationToken);

			    if (testFixtureClass == null)
			    {
				    return;
			    }

				var diagnosticVisitor = new TestFixtureMemberVisitor(fixtureMemberName, ctx, ws.CurrentSolution, testMethodParameterTypes);

			    var g = testFixtureClass
				    .DeclaringSyntaxReferences
					.SelectMany(syntaxRef => syntaxRef
									.GetSyntax(ctx.CancellationToken)
									.As<ClassDeclarationSyntax>()
									.Value
									.Members
									.Where(m => m is MethodDeclarationSyntax || m is PropertyDeclarationSyntax)
							   )
					.Select(d => d.Accept(diagnosticVisitor))
					.ToArray();
		    }
		}

		private INamedTypeSymbol FindClassDeclaration(Workspace workspace, string name, CancellationToken ct)
		{
			return workspace
				.CurrentSolution
				.Projects
				.Select(async p => await SymbolFinder.FindDeclarationsAsync(p, name, false, SymbolFilter.Type, ct))
				.WhenAll()
				.ContinueWith(t => t
					.Result
					.SelectMany(x => x)
					.FirstOrDefault()
					.As<INamedTypeSymbol>()
					.Value, ct)
                .Result;
		}

		private class TestFixtureMemberVisitor : CSharpSyntaxVisitor<Optional<Diagnostic>>
		{
			private readonly string _testFixtureMemberName;
			private readonly SyntaxNodeAnalysisContext _ctx;
			private readonly Solution _sln;
			private readonly ITypeSymbol[] _testMethodParameterTypes;

			public TestFixtureMemberVisitor(string testFixtureMemberName, SyntaxNodeAnalysisContext ctx, Solution sln, ITypeSymbol[] testMethodParameterTypes)
			{
				Contract.Requires(!string.IsNullOrWhiteSpace(testFixtureMemberName));

				_testFixtureMemberName = testFixtureMemberName;
				_ctx = ctx;
				_sln = sln;
				_testMethodParameterTypes = testMethodParameterTypes;
			}

			public override Optional<Diagnostic> VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				if (node.Identifier.ToFullString() != _testFixtureMemberName)
				{
					return new Optional<Diagnostic>();
				}

				//if (((GenericNameSyntax)node.ReturnType).Identifier.ToFullString().Equals("TheoryData"))
				//{
				//}

				var returnStatements = node
					.Body
					.Statements
					.Where(s => s is ReturnStatementSyntax || s is YieldStatementSyntax);

				foreach (var returnStatement in returnStatements)
				{
					var yieldReturn = returnStatement is YieldStatementSyntax;
					var retExpression = yieldReturn
				        ? ((YieldStatementSyntax) returnStatement).Expression
				        : ((ReturnStatementSyntax) returnStatement).Expression;
                    var retExprVisitor = new ReturnExpressionVisitor(_testFixtureMemberName, _ctx, _sln, _testMethodParameterTypes, yieldReturn);
				    var parameterTypes = retExpression.Accept(retExprVisitor);

					var b = 1;

					foreach (var parameterType in parameterTypes)
					{
						if (_testMethodParameterTypes.FirstOrDefault(x => x.Equals(parameterType)) == null)
						{
							var a = 1;
						}
					}
				}

				return base.VisitMethodDeclaration(node);
			}

			public override Optional<Diagnostic> VisitPropertyDeclaration(PropertyDeclarationSyntax node)
			{
				return base.VisitPropertyDeclaration(node);
			}
		}

		private class ReturnExpressionVisitor : CSharpSyntaxVisitor<IReadOnlyCollection<ITypeSymbol>>
        {
	        private readonly SemanticModel _semantic;
			private readonly string _testFixtureMemberName;
			private readonly SyntaxNodeAnalysisContext _ctx;
			private readonly Solution _sln;
			private readonly ITypeSymbol[] _testMethodParameterTypes;
			private readonly bool _yield;

			public ReturnExpressionVisitor(
				string testFixtureMemberName, 
				SyntaxNodeAnalysisContext ctx, 
				Solution sln, 
				ITypeSymbol[] testMethodParameterTypes, 
				bool yieldReturn)
			{
				_semantic = ctx.SemanticModel;
				_testFixtureMemberName = testFixtureMemberName;
				_ctx = ctx;
				_sln = sln;
				_testMethodParameterTypes = testMethodParameterTypes;
				_yield = yieldReturn;
			}

			public override IReadOnlyCollection<ITypeSymbol> VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
	        {
		        var t = node
			        .Initializer
			        .Expressions
			        .Select(expr => _semantic.GetTypeInfo(expr).Type)
			        .ToList();

				return node
			        .Initializer
			        .Expressions
			        .Select(expr => _semantic.GetTypeInfo(expr).Type)
					.ToList();
            }

	        public override IReadOnlyCollection<ITypeSymbol> VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
	        {
		        return base.VisitObjectCreationExpression(node);
	        }

	        private IEnumerable<object[]> T()
	        {
				return new List<object[]> {};
	        }

	        public override IReadOnlyCollection<ITypeSymbol> VisitInvocationExpression(InvocationExpressionSyntax node)
	        {
				var invocationType = _semantic.GetTypeInfo(node).Type;
		        
		        var all = invocationType.AllInterfaces.ToArray().Select(x => x.Name.Equals("IEnumerable"));
		        var tk = invocationType.TypeKind;
				//TODO: Consider compilation error case, when invocation expression return type are not equals IEnumerable<T>, etc.

				if (_yield)
				{
					if (!invocationType.AllInterfaces.Any(x => x.Name.Equals("IEnumerable")))
					{}

					var q = _semantic.GetSymbolInfo(node);
					
					var visitor = new SymVisitor();
					var decl = (MethodDeclarationSyntax)q.Symbol.Accept(visitor);
					var visitor1 = new TestFixtureMemberVisitor(decl.Identifier.ToFullString(), _ctx, _sln, _testMethodParameterTypes);
					decl.Accept(visitor1);
				}

		        var si = _semantic.GetSymbolInfo(node);
		        var declaredSym = _semantic.GetDeclaredSymbol(node);
				
				return new List<ITypeSymbol> {_semantic.GetTypeInfo(node).Type };
	        }

	        public override IReadOnlyCollection<ITypeSymbol> VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
	        {
		        return new List<ITypeSymbol> { _semantic.GetTypeInfo(node).Type };
	        }
        }

		private class SymVisitor : SymbolVisitor<SyntaxNode>
		{
			public override SyntaxNode VisitMethod(IMethodSymbol symbol)
			{
				Func<MemberDeclarationSyntax, bool> p = syntax =>
				{
					var someMethodDeclaration = syntax.As<MethodDeclarationSyntax>();

					return someMethodDeclaration.HasValue && someMethodDeclaration.Value.Identifier.ToFullString().Equals(symbol.Name);
				};
					
				return symbol
					.ReceiverType
					.DeclaringSyntaxReferences
					.FirstOrDefault(x => x
						.GetSyntax()
						.As<TypeDeclarationSyntax>()
						.Value
						.Members
						.Any(p))
					.GetSyntax()
					.As<TypeDeclarationSyntax>()
					.Value.Members.First(p);
			}
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
