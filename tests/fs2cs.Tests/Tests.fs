module fs2cs.Tests

open fs2cs
open NUnit.Framework
open Serilog

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
let ``empty () compile works`` () =
  let compiled = Library.main [|"--code";"()"|]
  //Log.Debug( "{@Compiled}", compiled )
  Assert.NotNull(compiled)
  Assert.IsEmpty(compiled.Errors)
