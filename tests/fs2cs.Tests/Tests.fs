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

[<Test>]
let ``tests works`` () =
  Directory.GetFiles( "../../", "test*.fsx" ) 
  |> Seq.map ( fun p -> Path.GetFullPath(p) )
  |> Seq.iter( fun source ->
    printfn "Testing %s ..." source
    let compiled =  Library.main [|"--projFile";source|]
    Assert.NotNull(compiled)
    Assert.IsNotEmpty(compiled)
    let a = compiled |> Seq.toArray
    Assert.AreEqual( 1, a.Length )
    let content = ( a.[0] |> snd ).ToString()
    let csharp = File.ReadAllText( Path.ChangeExtension( source, ".cs" ) )
    Assert.AreEqual( csharp, content )  
  )

[<Test>]
[<Ignore>]
let ``complex fsx test`` () =
  //try
    let compiled = Library.main [|"--projFile";"../../test.fsx"|]
    Assert.NotNull(compiled)
    Assert.IsNotEmpty(compiled)
    let a = compiled |> Seq.toArray
    Assert.AreEqual( 1, a.Length )
    let content = (snd  a.[0]).ToString()
    let file = fst a.[0]
    Assert.AreEqual( sprintf "public class %s {\r\n    public static readonly int x = 10 + 12 - 3;\r\n}" file.Root.Name, content )

[<Test>]
[<Ignore>]
let ``simple "printf \"%A\" 5" compile works`` () =
  //try
    let compiled = Library.main [|"--code";"printf \"%A\" 5"|]
    Assert.NotNull(compiled)
    Assert.IsNotEmpty(compiled)
    let a = compiled |> Seq.toArray
    Assert.AreEqual( 1, a.Length )
    let content = (snd  a.[0]).ToString()
    let file = fst a.[0]
    Assert.AreEqual( sprintf "public class %s {\r\n    public static int add(int a, int b) {\r\n        return a + b;\r\n    }\r\n}" file.Root.Name, content )    

[<Test>]
let ``simple "let a = System.Math.Abs(-5);;a" compile works`` () =
  //try
    let compiled = Library.main [|"--code";"let a = System.Math.Abs(-5);;a"|]
    Assert.NotNull(compiled)
    Assert.IsNotEmpty(compiled)
    let a = compiled |> Seq.toArray
    Assert.AreEqual( 1, a.Length )
    let content = (snd  a.[0]).ToString()
    let file = fst a.[0]
    Assert.AreEqual( sprintf "public class %s {\r\n    public static int add(int a, int b) {\r\n        return a + b;\r\n    }\r\n}" file.Root.Name, content )    

