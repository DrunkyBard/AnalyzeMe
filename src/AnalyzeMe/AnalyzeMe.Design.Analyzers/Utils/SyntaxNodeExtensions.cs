using Microsoft.CodeAnalysis;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static class SyntaxNodeExtensions
    {
        public static bool TryGetWorkspace(this SyntaxNode node, out Workspace workspace)
        {
            var syntaxTreeContainer = node.SyntaxTree.GetText().Container;
            return Workspace.TryGetWorkspace(syntaxTreeContainer, out workspace);
        }
    }
}
