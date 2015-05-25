using System;
using System.Collections.Generic;
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
                .Select(project => Task.Run(async () => await FindDerivedTypesAsync(project, type)))
                .ToList();
            await Task.WhenAll(findDerivedTypeTasks).ConfigureAwait(false);

            return findDerivedTypeTasks.SelectMany(x => x.Result);
        }

        private static async Task<IReadOnlyCollection<INamedTypeSymbol>> FindDerivedTypesAsync(Project project, INamedTypeSymbol classSymbol)
        {
            var compilation = await project.GetCompilationAsync().ConfigureAwait(false);
            var allTypes = GetContainingTypes(compilation.GlobalNamespace);

            return null;
        }

        private static IReadOnlyCollection<INamedTypeSymbol> GetContainingTypes(INamespaceSymbol nsSymbol)
        {
            var visitor = new NamespaceOrNamedTypeVisitor();
            var namedTypes = visitor.Visit(nsSymbol);

            return namedTypes;
        }

        //public static bool InheritsFromIgnoringConstruction(this ITypeSymbol type, ITypeSymbol baseType)
        //{
        //    var originalBaseType = baseType.OriginalDefinition;

        //    var currentBaseType = type.BaseType;
        //    while (currentBaseType != null)
        //    {
        //        if (EqualsCore(currentBaseType.OriginalDefinition, originalBaseType))
        //        {
        //            return true;
        //        }

        //        currentBaseType = currentBaseType.BaseType;
        //    }

        //    return false;
        //}

        private static Tuple<ISymbol, SymbolKind> GetKindAndUnwrapAlias(ISymbol symbol)
        {
            var kind = symbol.Kind;
            var returnSymbol = symbol;

            if (kind == SymbolKind.Alias)
            {
                returnSymbol = ((IAliasSymbol)symbol).Target;
                kind = symbol.Kind;
            }

            return new Tuple<ISymbol, SymbolKind>(returnSymbol, kind);
        }
    }

    internal class NamespaceOrNamedTypeVisitor : SymbolVisitor<IReadOnlyCollection<INamedTypeSymbol>>
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

            return Enumerable.Empty<INamedTypeSymbol>().ToList();
        }
    }
}
