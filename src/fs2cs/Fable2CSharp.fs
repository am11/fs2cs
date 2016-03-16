namespace fs2cs.Fable2CSharp

module Compiler =
    open System
    open System.IO
    open System.Text
    open Fable
    open Fable.AST.Fable
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.Formatting
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Formatting
    open Microsoft.CodeAnalysis.CSharp.Syntax

    let transformFiles (com: ICompiler) (files: File list) =
      files |> Seq.map( fun file ->
        let workspace = new AdhocWorkspace()
        let options = workspace.Options
        let options = options.WithChangedOption( CSharpFormattingOptions.NewLinesForBracesInMethods, false );
        let options = options.WithChangedOption( CSharpFormattingOptions.NewLinesForBracesInTypes, false );
        let compilationUnit = SyntaxFactory.CompilationUnit()

        let transformer = Transformer()
        let compilationUnit = transformer.Transform( file, compilationUnit, workspace, options )

        let sb = new StringBuilder()
        let writer = new StringWriter( sb )
        let formattedNode = Formatter.Format( compilationUnit, workspace, options );
        formattedNode.WriteTo( writer );

        file,writer
      )