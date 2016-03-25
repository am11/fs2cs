module fs2cs.Tests

open System
open fs2cs
open NUnit.Framework
open Serilog
open Microsoft.FSharp.Compiler.SourceCodeServices
open Newtonsoft.Json
open FSharp.Reflection
open Newtonsoft.Json.Serialization
open System.IO      

let checker = FSharpChecker.Create(keepAssemblyContents=true)

let parse projFile =
    let options =
        match Path.GetExtension(projFile) with
        | ".fsx" ->
            let projCode = File.ReadAllText projFile
            checker.GetProjectOptionsFromScript(projFile, projCode)
            |> Async.RunSynchronously
        | ".fsproj" ->
            ProjectCracker.GetProjectOptionsFromProjectFile(Path.GetFullPath projFile)
        | ext -> failwithf "Unexpected extension: %s" ext
    options
    |> checker.ParseAndCheckProject
    |> Async.RunSynchronously

let rec printDecls prefix (sw:StringWriter) decls  =
    decls |> Seq.iteri (fun i decl ->
        match decl with
          | FSharpImplementationFileDeclaration.Entity (e, sub) ->
              sw.WriteLine( sprintf "%s%i) ENTITY: %s" prefix i e.DisplayName)
              printDecls (prefix + "\t") sw sub 
          | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue (meth, args, body) ->
              if meth.IsCompilerGenerated |> not then
                  sw.WriteLine(sprintf "%s%i) METHOD: %s" prefix i meth.DisplayName)
                  // match body with
                  // | NewDelegate(_, Lambda(arg1, Lambda(arg2, _))) ->
                  //     printfn "arg1 = arg2 %A" (arg1.IsCompilerGenerated)
                  // | _ -> ()
                  sw.WriteLine(sprintf "%A" body)
          | FSharpImplementationFileDeclaration.InitAction (expr) ->
              sw.WriteLine(sprintf "%s%i) ACTION" prefix i)
              sw.WriteLine(sprintf "%A" expr)
        )


[<SetUpFixture>]
type SetupTest() =
    [<SetUp>]
    let ``start logging`` =
        Log.Logger <- LoggerConfiguration()
            .Destructure.FSharpTypes()
            .MinimumLevel.Debug() 
            .WriteTo.File("log.txt")
            .CreateLogger()
        Log.Information "Logging started"

let runTest n =
    let source = Path.GetFullPath("../../test" + n + ".fsx") 

    printfn "Testing %s ..." source

    let sw = new StringWriter()
    let proj = parse source
    proj.AssemblyContents.ImplementationFiles
    |> Seq.iteri (fun i file -> printfn "%i) %s" i file.FileName)
    proj.AssemblyContents.ImplementationFiles.[0].Declarations
    |> printDecls "" sw

    File.WriteAllText( Path.ChangeExtension( source, ".ast"),  sw.ToString() )
    let compiled =  Library.main [|"--projFile";source|]
    Assert.NotNull(compiled)
    Assert.IsNotEmpty(compiled)
    let a = compiled |> Seq.toArray
    Assert.AreEqual( 1, a.Length )
    let content = ( a.[0] |> snd ).ToString().Replace( "\r\n", "\n" )
    File.WriteAllText( Path.ChangeExtension( source, ".cs1"),  content )

    let csharp = File.ReadAllText( Path.ChangeExtension( source, ".cs" ) ).ToString().Replace( "\r\n", "\n" )
    Assert.AreEqual( csharp, content )  

    let references = [|
        typeof<System.Func<Object>>.Assembly.Location;
        typeof<System.Runtime.CompilerServices.DynamicAttribute>.Assembly.Location
        typeof<Microsoft.CSharp.RuntimeBinder.Binder>.Assembly.Location
        typeof<fs2csLib.Impl>.Assembly.Location
    |]

    let compiler = 
        new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider()

    let compilerParams = 
        new System.CodeDom.Compiler.CompilerParameters(references)
    
    let result = compiler.CompileAssemblyFromSource(compilerParams, content)
    Assert.IsEmpty(result.Errors)

[<Test>]
let ``test 1`` () = runTest "1"

[<Test>] 
let ``test 2`` () = runTest "2"

[<Test>] 
let ``test 3`` () = runTest "3"

[<Test>]
let ``test 4`` () = runTest "4"

[<Test>]
let ``test 5`` () = runTest "5"

[<Test>]
let ``test 6`` () = runTest "6"

[<Test>]
let ``test 7`` () = runTest "7"

[<Test>]
let ``test 8`` () = runTest "8"

[<Test>]
let ``test 9`` () = runTest "9"

[<Test>]
let ``test 10`` () = runTest "10"

[<Test>]
let ``test 11`` () = runTest "11"

[<Test>]
let ``test 12`` () = runTest "12"

[<Test>]
let ``test 13`` () = runTest "13"

[<Test>]
let ``Multiplication, passing function as a parameter`` () = runTest "14"

[<Test>]
let ``SimpleArithmetic with printf`` () = runTest "15"

[<Test>]
let ``printf`` () = runTest "16"

[<Test>]
let ``Arrays`` () = runTest "17"

[<Test>]
let ``if`` () = runTest "18"
