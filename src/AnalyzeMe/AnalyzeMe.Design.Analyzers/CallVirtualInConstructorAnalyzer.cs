using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzeMe.Design.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CallVirtualInConstructorAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            throw new System.NotImplementedException();
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
    }
}
