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
        let workspace = AdhocWorkspace()
        let options = workspace.Options
        let options = options.WithChangedOption( CSharpFormattingOptions.NewLinesForBracesInMethods, false );
        let options = options.WithChangedOption( CSharpFormattingOptions.NewLinesForBracesInTypes, false );
        let compilationUnit = SyntaxFactory.CompilationUnit()

        let compilationUnit = compilationUnit.AddUsings( SyntaxFactory.UsingDirective( SyntaxFactory.IdentifierName( "System" ) ) )
        let classDeclaration = SyntaxFactory.ClassDeclaration( file.Root.Name )
                
        let members = 
          file.Declarations |> Seq.map( fun declaration ->
            match declaration with
            | MemberDeclaration(member1) ->
                let kind = member1.Kind
                match kind with
                | Getter(name,isField) -> 
                    let propertyDeclaration = 
                      SyntaxFactory.PropertyDeclaration( 
                        SyntaxFactory.IdentifierName( member1.Body.Type.FullName ), file.Root.Name 
                      )
                    let result : MemberDeclarationSyntax = upcast propertyDeclaration
                    result
                | _ -> 
                  let propertyDeclaration = 
                    SyntaxFactory.PropertyDeclaration( 
                      SyntaxFactory.IdentifierName( "String" ), kind.ToString()
                    )
                  let result : MemberDeclarationSyntax = upcast propertyDeclaration
                  result
             | _ -> 
                  let propertyDeclaration = 
                    SyntaxFactory.PropertyDeclaration( 
                      SyntaxFactory.IdentifierName( "String" ), declaration.ToString()
                    )
                  let result : MemberDeclarationSyntax = upcast propertyDeclaration
                  result
          )
          |> Seq.toArray 
        let classDeclaration = classDeclaration.AddMembers(members)
        let compilationUnit = compilationUnit.AddMembers( classDeclaration )

        let sb = new StringBuilder()
        let writer = new StringWriter( sb )
        let formattedNode = Formatter.Format( compilationUnit, workspace, options );
        formattedNode.WriteTo( writer );

        writer
      )