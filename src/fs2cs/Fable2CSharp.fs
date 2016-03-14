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

        let compilationUnit = compilationUnit.AddUsings( SyntaxFactory.UsingDirective( SyntaxFactory.IdentifierName( "System" ) ) )
        let classDeclaration = SyntaxFactory.ClassDeclaration( file.Root.Name )
                
        let members = 
          file.Declarations |> Seq.map( fun declaration ->
            match declaration with
            | MemberDeclaration(member1) ->
                let kind = member1.Kind
                match kind with
                | Getter(name,isField) -> 
                    None
                    (*
                    let propertyDeclaration = 
                      SyntaxFactory.FieldDeclaration( 
                        //SyntaxFactory.IdentifierName( member1.Body.Type.FullName ), name
                        SyntaxFactory.VariableDeclaration( SyntaxFactory.IdentifierName( SyntaxFactory.Identifier(name) ) )
                      )
                    let propertyDeclaration = propertyDeclaration.WithModifiers( SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)) )
                    let propertyDeclaration = propertyDeclaration.WithModifiers( SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)) )
                    let propertyDeclaration = propertyDeclaration.WithSemicolonToken
                    let result : MemberDeclarationSyntax = upcast propertyDeclaration
                    Some(result)
                    *)
                | _ -> None
                  
             | _ -> None
          )
          |> Seq.filter( fun p -> p.IsSome) |> Seq.map( fun p->p.Value) |> Seq.toArray 
        let classDeclaration = classDeclaration.AddMembers(members)
        let compilationUnit = compilationUnit.AddMembers( classDeclaration )

        let sb = new StringBuilder()
        let writer = new StringWriter( sb )
        let formattedNode = Formatter.Format( compilationUnit, workspace, options );
        formattedNode.WriteTo( writer );

        writer
      )