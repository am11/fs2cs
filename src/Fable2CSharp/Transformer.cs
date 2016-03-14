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

namespace Fable2CSharp
{
    public class Transformer
    {
        public CompilationUnitSyntax Transform(File file, CompilationUnitSyntax compilationUnit, AdhocWorkspace workspace, OptionSet options )
        {
            return compilationUnit;
        }
    }
}
