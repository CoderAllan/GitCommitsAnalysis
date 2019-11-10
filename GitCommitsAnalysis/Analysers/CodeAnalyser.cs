using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace GitCommitsAnalysis.Analysers
{
    public static class CodeAnalyser
    {
        public static SyntaxTree GetSyntaxTree(string fileContents)
        {
            var tree = CSharpSyntaxTree.ParseText(fileContents);
            return tree;
        }
        public static IEnumerable<MethodDeclarationSyntax> GetMethodDeclarationSyntaxe(SyntaxTree tree)
        {
            var syntaxNode = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>();
            return syntaxNode;
        }

        public static SemanticModel GetModel(SyntaxTree tree)
        {
            var compilation = CSharpCompilation.Create(
                "x",
                syntaxTrees: new[] { tree },
                references:
                new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(typeof (object).Assembly.Location)
                });

            var model = compilation.GetSemanticModel(tree, true);
            return model;
        }
    }
}
