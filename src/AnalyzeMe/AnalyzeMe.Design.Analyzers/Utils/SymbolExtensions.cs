using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnalyzeMe.Design.Analyzers.Utils;
using Microsoft.CodeAnalysis;
using QAlias = AnalyzeMe.Design.Analyzers.Utils.Q;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    class Q
    {
    }

    class C<A, B>
    {
         
    }

    class C1 : C<int[], QAlias>
    {
    }


    public static class SymbolExtensions
    {
        public static async Task<IEnumerable<INamedTypeSymbol>> FindDerivedClassesAsync(
            this INamedTypeSymbol type,
            Solution solution,
            CancellationToken cancellationToken)
        {
            if (type == null || type.TypeKind == TypeKind.Class && type.IsSealed)
            {
                return await Task.FromResult(Enumerable.Empty<INamedTypeSymbol>());
            }

            var findDerivedTypeTasks = solution
                .Projects
                .Select(project => FindDerivedTypesAsync(project, type, cancellationToken))
                .ToList();
            await Task.WhenAll(findDerivedTypeTasks).ConfigureAwait(false);

            return findDerivedTypeTasks.SelectMany(x => x.Result);
        }

        private static async Task<IReadOnlyCollection<INamedTypeSymbol>> FindDerivedTypesAsync(Project project, INamedTypeSymbol classSymbol, CancellationToken cancellationToken)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            var allTypes = GetContainingTypes(compilation.GlobalNamespace)
                .Where(potentialDerivedType => potentialDerivedType.InheritsFromIgnoringConstruction(classSymbol))
                .ToList();

            return null;
        }

        private static IReadOnlyCollection<INamedTypeSymbol> GetContainingTypes(INamespaceSymbol nsSymbol)
        {
            var visitor = new NamespaceOrNamedTypeVisitor();
            var namedTypes = visitor.Visit(nsSymbol);

            return namedTypes;
        }

        public static bool InheritsFromIgnoringConstruction(this INamedTypeSymbol type, INamedTypeSymbol baseType)
        {
            if (type.TypeKind != baseType.TypeKind)
            {
                return false;
            }

            var originalBaseType = baseType.OriginalDefinition;
            
            var currentBaseType = type.BaseType;
            while (currentBaseType != null)
            {
                var definitionOfCurrentBaseType = currentBaseType.OriginalDefinition;

                if (definitionOfCurrentBaseType.Equals(definitionOfCurrentBaseType.ConstructedFrom) == originalBaseType.Equals(originalBaseType.ConstructedFrom)
                    && definitionOfCurrentBaseType.IsDefinition == originalBaseType.IsDefinition
                    && definitionOfCurrentBaseType.Name == originalBaseType.Name
                    && definitionOfCurrentBaseType.IsAnonymousType == originalBaseType.IsAnonymousType
                    && definitionOfCurrentBaseType.IsUnboundGenericType == originalBaseType.IsUnboundGenericType
                    && (definitionOfCurrentBaseType.ContainingAssembly == null || originalBaseType.ContainingAssembly == null || definitionOfCurrentBaseType.ContainingAssembly.Identity.ToString() == originalBaseType.ContainingAssembly.Identity.ToString())
                    && CheckTypeArguments(definitionOfCurrentBaseType.TypeArguments, originalBaseType.TypeArguments))
                {
                    return true;
                }
                
                currentBaseType = currentBaseType.BaseType;
            }

            return false;
        }

        private static bool CheckTypeArguments(ImmutableArray<ITypeSymbol> x, ImmutableArray<ITypeSymbol> y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }

            var visitor = new TypeArgumentVisitor();

            for (int i = 0; i < x.Length; i++)
            {
                visitor.Visit(x[i], y[i]);
            }

            return true;
        }

        private static Tuple<ISymbol, SymbolKind> UnwrapClassDeclarationSymbol(ISymbol symbol)
        {
            var kind = symbol.Kind;
            var returnSymbol = symbol;

            if (kind == SymbolKind.Alias)
            {
                returnSymbol = ((IAliasSymbol)symbol).Target;
            }

            return new Tuple<ISymbol, SymbolKind>(returnSymbol, kind);
        }
    }

    internal sealed class NamespaceOrNamedTypeVisitor : SymbolVisitor<IReadOnlyCollection<INamedTypeSymbol>>
    {
        public override IReadOnlyCollection<INamedTypeSymbol> VisitNamespace(INamespaceSymbol symbol)
        {
            var members = symbol
                .GetMembers()
                .ToList();
            var a = members.SelectMany(Visit)
            .ToList();

            return a;
        }

        public override IReadOnlyCollection<INamedTypeSymbol> VisitNamedType(INamedTypeSymbol symbol)
        {
            var canBeReferenced = symbol.CanBeReferencedByName;
            var isInSource = symbol.Locations.Any(loc => loc.IsInSource);

            if (isInSource)
            {
                return new List<INamedTypeSymbol> {symbol};
            }
            //
            return Enumerable.Empty<INamedTypeSymbol>().ToList();
        }
    }

    internal sealed class TypeArgumentVisitor : SymbolVisitor
    {
        private ITypeSymbol _baseTypeSymbol;

        public void Visit(ITypeSymbol currentBaseType, ITypeSymbol originBaseType)
        {
            //TODO: Check all constraints via ITypeParameterSymbol properties
            _baseTypeSymbol = originBaseType;
            Visit(currentBaseType);
        }

        public override void VisitTypeParameter(ITypeParameterSymbol symbol)
        {
            var res = _.Match(
                match: _baseTypeSymbol.As<ITypeParameterSymbol>(),
                some: x => true,
                none: () => false);

            base.VisitTypeParameter(symbol);
        }
    }
}
