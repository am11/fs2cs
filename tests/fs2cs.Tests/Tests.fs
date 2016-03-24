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

type ErasedUnionConverter() =
    inherit JsonConverter()
    override x.CanConvert t =
        t.Name = "FSharpOption`1" ||
        FSharpType.IsUnion t &&
            t.GetCustomAttributes true
            |> Seq.exists (fun a -> (a.GetType ()).Name = "EraseAttribute")
    override x.ReadJson(reader, t, v, serializer) =
        failwith "Not implemented"
    override x.WriteJson(writer, v, serializer) =
        match FSharpValue.GetUnionFields (v, v.GetType()) with
        | _, [|v|] -> serializer.Serialize(writer, v) 
        | _ -> writer.WriteNull()  
        
type CustomResolver() =
    inherit DefaultContractResolver()
    override x.CreateProperty(member1, memberSerialization)  =
      let property = base.CreateProperty(member1, memberSerialization)
      let badPropNames = 
        [| "FSharpDelegateSignature"; "QualifiedName"; "FullName"; "AbbreviatedType";
           "GenericParameter"; "GetterMethod"; "EventAddMethod"; "EventRemoveMethod";
           "EventDelegateType"; "EventIsStandard"; "SetterMethod"; "NamedEntity";
           "TypeDefinition"
        |]
      if badPropNames |> Seq.exists( fun p -> p = property.PropertyName ) then 
        property.ShouldSerialize <- ( fun _ -> false )
      property
      

let printCs fileName par =
    let file = new StreamWriter(fileName+".cs")
    file.WriteLine(par.ToString())
    file.Close()

let printJson fileName par =
    let jsonSettings = 
        JsonSerializerSettings(
            Converters=[|ErasedUnionConverter()|],
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            StringEscapeHandling=StringEscapeHandling.EscapeNonAscii)
    jsonSettings.ContractResolver <- CustomResolver()
    let result = JsonConvert.SerializeObject (par, jsonSettings)
    File.WriteAllText(fileName+".json", result )

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
