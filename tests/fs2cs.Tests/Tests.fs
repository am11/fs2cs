module fs2cs.Tests

open fs2cs
open NUnit.Framework
open Serilog
open Microsoft.FSharp.Compiler.SourceCodeServices

[<SetUpFixture>]
type SetupTest() =
    [<SetUp>]
    let ``start logging`` =
        Log.Logger <- LoggerConfiguration()
            .Destructure.FSharpTypes()
            .MinimumLevel.Debug() 
            .WriteTo.ColoredConsole()
            .CreateLogger()
        Log.Information( "Tests started" )

[<Test>]
let ``hello returns 42`` () =
  let result = Library.hello 42
  printfn "%i" result
  Assert.AreEqual(42,result)

[<Test>]
let ``empty "()" compile works`` () =
  let compiled = Library.main [|"--code";"()"|]
  //Log.Debug( "{@Compiled}", compiled )
  Assert.NotNull(compiled)
  Assert.IsEmpty(compiled.Errors)

[<Test>]
let ``simple "let a=12345" compile works`` () =
  let compiled = Library.main [|"--code";"let a=12345"|]
  //Log.Debug( "{@Compiled}", compiled )
  Assert.NotNull(compiled)
  Assert.IsEmpty(compiled.Errors)
  Assert.IsNotEmpty( compiled.AssemblyContents.ImplementationFiles )
  let declarations = compiled.AssemblyContents.ImplementationFiles.Head.Declarations
  Assert.IsNotEmpty( declarations )
  let declaration = declarations.Head
  match declaration with
  | Entity(x,y) -> 
      match y.Head with 
      | MemberOrFunctionOrValue(a, b, c) -> Assert.AreEqual("a",a.DisplayName)
      | _ -> Assert.Fail(sprintf "OTHER: %A" y)
  | _ -> Assert.Fail(sprintf "OTHER: %A" declaration)

[<Test>]
let ``simple "printfn \"Hello\"" compile works`` () =
  let compiled = Library.main [|"--code";"printfn \"Hello\""|]
  Assert.NotNull(compiled)
  Assert.IsEmpty(compiled.Errors)
  Assert.IsNotEmpty( compiled.AssemblyContents.ImplementationFiles )
  let declarations = compiled.AssemblyContents.ImplementationFiles.Head.Declarations
  Assert.IsNotEmpty( declarations )
  let declaration = declarations.Head
  match declaration with
  | Entity(x,y) -> 
      match y.Head with 
      | MemberOrFunctionOrValue(a, b, c) -> Assert.AreEqual("a",a.DisplayName)
      | _ -> Assert.Fail(sprintf "OTHER: %A" y)
  | _ -> Assert.Fail(sprintf "OTHER: %A" declaration)
  Log.Debug( "{@declarations}", declarations )  
