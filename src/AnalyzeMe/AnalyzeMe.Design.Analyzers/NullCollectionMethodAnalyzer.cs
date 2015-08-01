using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzeMe.Design.Analyzers
{
    /// <summary>
    /// Checks whether the method returns an null collection.
    /// </summary>
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    //public sealed class NullCollectionMethodAnalyzer : DiagnosticAnalyzer
    //{
    //    public const string NullCollectionDiagnosticId = "Placeholder";
    //    internal static readonly LocalizableString NullCollectionTitle = "Placeholder";
    //    internal static readonly LocalizableString NullCollectionMessageFormat = "Placeholder";
    //    internal const string NullCollectionCategory = "Design";
    //    internal static readonly DiagnosticDescriptor NullCollectionRule = new DiagnosticDescriptor(
    //        NullCollectionDiagnosticId,
    //        NullCollectionTitle,
    //        NullCollectionMessageFormat,
    //        NullCollectionCategory,
    //        DiagnosticSeverity.Warning,
    //        isEnabledByDefault: true);
    //    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NullCollectionRule);

    //    public override void Initialize(AnalysisContext context)
    //    {
    //        context.RegisterSyntaxNodeAction(x => { }, SyntaxKind.MethodDeclaration, SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration);
    //    }
    //}
}
