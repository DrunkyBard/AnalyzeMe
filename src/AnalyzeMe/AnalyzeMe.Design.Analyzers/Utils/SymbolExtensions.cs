using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace AnalyzeMe.Design.Analyzers.Utils
{
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

        private static async Task<IReadOnlyCollection<INamedTypeSymbol>> FindDerivedTypesAsync(
            Project project, 
            INamedTypeSymbol classSymbol, 
            CancellationToken cancellationToken)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            var allTypes = GetContainingTypes(compilation.GlobalNamespace)
                .Where(potentialDerivedType => potentialDerivedType.InheritsFromIgnoringConstruction(classSymbol))
                .ToList();

            return allTypes;
        }

        private static IReadOnlyCollection<INamedTypeSymbol> GetContainingTypes(INamespaceSymbol nsSymbol)
        {
            var visitor = new NamespaceOrNamedTypeVisitor();
            var namedTypes = visitor.Visit(nsSymbol);

            return namedTypes;
        }

        public static bool InheritsFromIgnoringConstruction(this INamedTypeSymbol potentialDerivedType, INamedTypeSymbol baseType)
        {
            if (potentialDerivedType.TypeKind != baseType.TypeKind)
            {
                return false;
            }

            var originalBaseType = baseType.OriginalDefinition;
            var currentBaseType = potentialDerivedType.BaseType;

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

            for (int i = 0; i < x.Length; i++)
            {
                var equals = CheckTypeParametersForEquality(x[i], y[i]);

                if (!equals)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckTypeParametersForEquality(ITypeSymbol currentBaseType, ITypeSymbol originBaseType)
        {
            var currentTypeParameter = currentBaseType as ITypeParameterSymbol;
            var originTypeParameter = originBaseType as ITypeParameterSymbol;

            if (currentTypeParameter == null || originTypeParameter == null)
            {
                return false;
            }

            var match =
                currentTypeParameter.HasConstructorConstraint == originTypeParameter.HasConstructorConstraint
                && currentTypeParameter.HasReferenceTypeConstraint == originTypeParameter.HasReferenceTypeConstraint
                && currentTypeParameter.HasValueTypeConstraint == originTypeParameter.HasValueTypeConstraint;

            return match;
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

            return members.SelectMany(Visit).ToList();
        }

        public override IReadOnlyCollection<INamedTypeSymbol> VisitNamedType(INamedTypeSymbol symbol)
        {
            var isInSource = symbol.Locations.Any(loc => loc.IsInSource);

            if (isInSource)
            {
                return new List<INamedTypeSymbol> {symbol};
            }
            
            return Enumerable.Empty<INamedTypeSymbol>().ToList();
        }
    }
}
