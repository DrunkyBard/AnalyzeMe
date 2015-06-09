using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace AnalyzeMe.Design.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullCollectionMethodCodeFixProvider)), Shared]
    public sealed class NullCollectionMethodCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NullCollectionMethodAnalyzer.NullCollectionDiagnosticId);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
