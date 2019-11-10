using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Zu.TypeScript;

namespace GitCommitsAnalysis.Analysers
{

    public static class MethodCounter
    {
        public static int Calculate(IEnumerable<MethodDeclarationSyntax> syntaxNode)
        {
            int result = syntaxNode.Count();
            return result;
        }

        public static int Calculate(TypeScriptAST typeScriptAst, string fileContents)
        {
            typeScriptAst.MakeAST(fileContents);
            var methods = typeScriptAst.OfKind(Zu.TypeScript.TsTypes.SyntaxKind.MethodDeclaration);
            var functions = typeScriptAst.OfKind(Zu.TypeScript.TsTypes.SyntaxKind.FunctionDeclaration);
            return functions.Count() + methods.Count();
        }
    }
}