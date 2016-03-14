using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Fable.AST.Fable;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace fs2cs.Fable2CSharp
{
    public class Transformer
    {
        public CompilationUnitSyntax Transform(File file, CompilationUnitSyntax compilationUnit, AdhocWorkspace workspace, OptionSet options)
        {
            return 
                compilationUnit
                .WithMembers(SingletonList<MemberDeclarationSyntax>(ClassDeclaration(file.Root.Name))).NormalizeWhitespace()
            ;
        }
    }
}
