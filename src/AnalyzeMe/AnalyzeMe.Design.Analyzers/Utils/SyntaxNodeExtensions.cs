using Microsoft.CodeAnalysis;

namespace AnalyzeMe.Design.Analyzers.Utils
{
    public static class SyntaxNodeExtensions
    {
        public static Workspace TryGetWorkspace(this SyntaxNode node)
        {
            var syntaxTreeContainer = node.SyntaxTree.GetText().Container;
            Workspace workspace;
            Workspace.TryGetWorkspace(syntaxTreeContainer, out workspace);

            return workspace;
        }
    }
}
