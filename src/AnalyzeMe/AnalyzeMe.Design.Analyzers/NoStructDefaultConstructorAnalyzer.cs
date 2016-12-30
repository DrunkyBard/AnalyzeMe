using System;
using System.Collections.Immutable;
using System.Linq;
using AnalyzeMe.Design.Analyzers.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;

namespace AnalyzeMe.Design.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class NoStructDefaultConstructorAnalyzer : DiagnosticAnalyzer
	{
		internal static readonly DiagnosticDescriptor NoStructDefaultConstructorDescription = new DiagnosticDescriptor(
			"NoStructDefaultConstructorId",
			"",
			"",
			"",
			DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		internal static readonly DiagnosticDescriptor StructHasAttributeButHasParameterlessConstructorOnly = new DiagnosticDescriptor(
			"StructHasAttributeButHasParameterlessConstructorOnlyId",
			"",
			"",
			"",
			DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoStructDefaultConstructorDescription, StructHasAttributeButHasParameterlessConstructorOnly);

		public override void Initialize(AnalysisContext context)
		{
			//context.RegisterSyntaxNodeAction(AnalyzeStructDefaultCtorInvocation, SyntaxKind.ObjectCreationExpression);
			context.RegisterSyntaxNodeAction(AnalyzeStructTypeParameter, SyntaxKind.GenericName);
			//context.RegisterSyntaxNodeAction(AnalyzeStructDefaultCtorInvocation, SyntaxKind.MethodDeclaration);
		}

		private void AnalyzeStructTypeParameter(SyntaxNodeAnalysisContext ctx)
		{
			var visitor = new GenericInvocationVisitor(ctx.SemanticModel);
			ctx
				.Node
				.Parent
				.As<CSharpSyntaxNode>()
				.Value
				.Accept(visitor)
				.WhenHasValueThen(ctx.ReportDiagnostic);
		}

		private void AnalyzeStructDefaultCtorInvocation(SyntaxNodeAnalysisContext ctx)
		{
			var objectCreation = ctx
				.Node
				.As<ObjectCreationExpressionSyntax>()
				.Value;

			if (objectCreation.ArgumentList.Arguments.Any())
			{
				return;
			}

			var symbolInfo = ctx.SemanticModel.GetSymbolInfo(objectCreation.Type);

			var namedSymbol = symbolInfo.Symbol?.As<INamedTypeSymbol>().Value;

			if (namedSymbol == null || !namedSymbol.IsValueType)
			{
				return;
			}

			var hasAttribute = namedSymbol
				.DeclaringSyntaxReferences
				.Select(x => x.GetSyntax())
				.OfType<StructDeclarationSyntax>()
				.Any(structDeclaration => structDeclaration
				.AttributeLists
				.SelectMany(x => x.Attributes)
				.Any(x =>
				{
					var attrSymbolInfo = ctx.SemanticModel.GetSymbolInfo(x, ctx.CancellationToken);

					return attrSymbolInfo.Symbol != null &&
						   ctx.SemanticModel
							  .GetSymbolInfo(x, ctx.CancellationToken).Symbol
							  .As<IMethodSymbol>()
							  .Value
							  .ReceiverType
							  .OriginalDefinition
							  .ToString()
							  .Equals("AnalyzeMe.WorkProcess.Tools.NoDefaultConstructorAttribute", StringComparison.Ordinal);
				}));

			var hasDefaultConstructorOnly = !namedSymbol.Constructors.Any(x => x.Arity > 0);

			if (hasAttribute)
			{
				var d1 = Diagnostic.Create(NoStructDefaultConstructorDescription, ctx.Node.GetLocation());
				ctx.ReportDiagnostic(d1);

				if (hasDefaultConstructorOnly)
				{
					var d2 = Diagnostic.Create(StructHasAttributeButHasParameterlessConstructorOnly, ctx.Node.GetLocation());
					ctx.ReportDiagnostic(d2);
				}
			}
		}

		private sealed class GenericInvocationVisitor : CSharpSyntaxVisitor<Optional<Diagnostic>>
		{
			private readonly SemanticModel _semantic;

			public GenericInvocationVisitor(SemanticModel semantic)
			{
				_semantic = semantic;
			}

			public override Optional<Diagnostic> VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
				var genericType = _semantic
					.GetSymbolInfo(node.Type)
					.Symbol
					.As<INamedTypeSymbol>()
					.Value;

				var structTypeSymbols = genericType
					.TypeParameters
					.Where(type => type.HasValueTypeConstraint)
					.ToArray();

				var q = genericType
					.DeclaringSyntaxReferences
					.Select(x => x.GetSyntax())
					.OfType<CSharpSyntaxNode>()
					.ToArray();

				Workspace ws;
				node.TryGetWorkspace(out ws);
				
				if (structTypeSymbols.Length == 0)
				{
					return new Optional<Diagnostic>();
				}

				foreach (var typeSymbol in structTypeSymbols)
				{
					var typeSyntax = typeSymbol.DeclaringSyntaxReferences[0].GetSyntax().As<TypeParameterSyntax>().Value;
					
					foreach (var cSharpSyntaxNode in q)
					{
						var ns = cSharpSyntaxNode
							.DescendantNodes()
							.OfType<ObjectCreationExpressionSyntax>()
							.ToList();
						cSharpSyntaxNode.Accept(new Walker(typeSyntax, _semantic));
					}

					var refs = SymbolFinder.FindReferencesAsync(typeSymbol, ws.CurrentSolution).Result;
					var refs1 = SymbolFinder.FindReferencesAsync(genericType, ws.CurrentSolution).Result;
				}

				return base.VisitObjectCreationExpression(node);
			}

			public override Optional<Diagnostic> VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				return base.VisitInvocationExpression(node);
			}

			public override Optional<Diagnostic> DefaultVisit(SyntaxNode node)
			{
				return new Optional<Diagnostic>();
			}
		}

		private sealed class Walker : CSharpSyntaxWalker
		{
			private readonly TypeParameterSyntax _originTypeParameter;
			private readonly SemanticModel _semantic;

			public Walker(TypeParameterSyntax originTypeParameter, SemanticModel semantic)
			{
				if (originTypeParameter == null)
				{
					throw new ArgumentNullException(nameof(originTypeParameter));
				}

				if (semantic == null)
				{
					throw new ArgumentNullException(nameof(semantic));
				}

				_originTypeParameter = originTypeParameter;
				_semantic = semantic;
			}

			public override void VisitDefaultExpression(DefaultExpressionSyntax node) => AreEquals(node.Type);

			public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node) => AreEquals(node.Type);

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				var invocationSymbol = _semantic.GetSymbolInfo(node);
				var methodSymbolOpt = invocationSymbol.Symbol.As<IMethodSymbol>();

				if (!methodSymbolOpt.HasValue)
				{
					return;
				}

				var methodSymbol = methodSymbolOpt.Value;
				var isActivatorDotCreateInstance =
					methodSymbol.ReceiverType.ToString().Equals("System.Activator", StringComparison.Ordinal) &&
					methodSymbol.Name.Equals("CreateInstance", StringComparison.Ordinal);

				var genericWithOnlyOneTypeParameter = methodSymbol.IsGenericMethod &&
				                                      methodSymbol.TypeParameters.Length == 1 &&
													  methodSymbol.Parameters.Length == 0;

				var nonGenericWithOnlyOneParameter = !methodSymbol.IsGenericMethod &&
				                                      methodSymbol.TypeParameters.Length == 0 &&
				                                      methodSymbol.Parameters.Length == 1;

				var ok = isActivatorDotCreateInstance && (genericWithOnlyOneTypeParameter || nonGenericWithOnlyOneParameter);

				if (!ok)
				{
					return;
				}

				if (genericWithOnlyOneTypeParameter)
				{
					var o = methodSymbol.TypeParameters[0];
				}
			}

			private bool AreEquals(TypeSyntax typeSyntax)
			{
				var typeIdentifier = typeSyntax.As<IdentifierNameSyntax>();

				return typeIdentifier.HasValue && _originTypeParameter.Identifier.IsEquivalentTo(typeIdentifier.Value.Identifier);
			}
		}
	}

	public struct A
	{
		public A(int i)
		{

		}
	}

	public class Q<T, U, P>
	{
		void D()
		{
			Activator.CreateInstance<string>();
		}
	}

	public class O<T> where T : new()
	{
	}

	public class W
	{
		public void A<T>() where T : new()
		{
			Activator.CreateInstance(typeof(int), 1);
			Activator.CreateInstance<int>();
			var g = default(T);
			new T();
		}
	}

	public class Test
	{
		public void A()
		{
			var q = new Q<A, string, int>();
			var w = new W();
			w.A<A>();
		}
	}
}
