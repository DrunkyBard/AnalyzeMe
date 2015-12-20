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
        public static IMethodSymbol GetOriginMethodSymbol(this IMethodSymbol overridenMethod)
        {
            var originMethod = overridenMethod;

            while (originMethod.OverriddenMethod != null)
            {
                originMethod = originMethod.OverriddenMethod;
            }

            return originMethod;
        }

        /// <summary>
        /// Find all derived types for <paramref name="type"/> symbol.
        /// </summary>
        /// <param name="type">Type symbol for which there is a search of all derived types.</param>
        /// <param name="solution">Current solution.</param>
        /// <param name="cancellationToken">Token for cancellation of the analysis.</param>
        /// <returns>Collection of derived types from <paramref name="type"/> symbol.</returns>
        /// <remarks>Note that method find all derieved types in current solution, only in source code, not in external libraries!</remarks>
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
                .Where(potentialDerivedType => potentialDerivedType.InheritsFrom(classSymbol))
                .ToList();

            return allTypes;
        }

        private static IReadOnlyCollection<INamedTypeSymbol> GetContainingTypes(INamespaceSymbol nsSymbol)
        {
            var visitor = new NamespaceOrNamedTypeVisitor();
            var namedTypes = visitor.Visit(nsSymbol);

            return namedTypes;
        }

        /// <summary>
        /// Recursively checks whether the <paramref name="potentialDerivedType"/> type symbol to be derived from <paramref name="baseType"/> type symbol.
        /// </summary>
        /// <param name="potentialDerivedType">Verifyable potential derived type.</param>
        /// <param name="baseType">Base type of <paramref name="potentialDerivedType"/>.</param>
        /// <returns>True, if <paramref name="potentialDerivedType"/> is derived type from <paramref name="baseType"/>, otherwise false.</returns>
        private static bool InheritsFrom(this INamedTypeSymbol potentialDerivedType, INamedTypeSymbol baseType)
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
    }
}
