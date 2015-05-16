using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzeMe.Design.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SealedClassAnalyzer : DiagnosticAnalyzer
    {
        public const string ClassCanBeSealedDiagnosticId = "MissingSealedModifier";
        internal static readonly LocalizableString ClassCanBeSealedTitle = "Class can be marked as sealed.";
        internal static readonly LocalizableString ClassCanBeSealedMessageFormat = "Class can be marked as sealed.";
        internal const string AttributeUsageCategory = "Design";
        internal static readonly DiagnosticDescriptor ClassCanBeSealedRule = new DiagnosticDescriptor(ClassCanBeSealedDiagnosticId, ClassCanBeSealedTitle, ClassCanBeSealedMessageFormat, AttributeUsageCategory, DiagnosticSeverity.Error, isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ClassCanBeSealedRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private async void AnalyzeClass(SyntaxNodeAnalysisContext ctx)
        {
        }
    }
}