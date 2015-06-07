using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AnalyzeMe.Design.Analyzers.Utils
{
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
            var isInSource = symbol.Locations.Any(loc => loc.IsInSource); //Check is in current solution, not in external library.

            if (isInSource)
            {
                return new List<INamedTypeSymbol> { symbol };
            }

            return Enumerable.Empty<INamedTypeSymbol>().ToList();
        }
    }
}