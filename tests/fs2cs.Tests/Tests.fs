module fs2cs.Tests

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
      

let print fileName par =
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

#if NONO
[<Test>]
let ``hello returns 42`` () =
  let result = Library.hello 42
  printfn "%i" result
  Assert.AreEqual(42,result)

[<Test>]
let ``empty "()" compile works`` () =
  let compiled = Library.main [|"--code";"()"|]
  Assert.NotNull(compiled)
  Assert.IsEmpty(compiled.Errors)
#endif

(*
[<Test>]
let ``simple "let a=12345" compile works`` () =
  let compiled = Library.main [|"--code";"let a=12345"|]
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
  //print compiled

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

[<Test>]
let ``simple "let a=123;;printfn \"%A\" a" compile works`` () =
  let compiled = Library.main [|"--code";"let a=123;;printfn \"%A\" a"|]
  Assert.NotNull(compiled)
  Assert.IsEmpty(compiled.Errors)
  Assert.IsNotEmpty( compiled.AssemblyContents.ImplementationFiles )
  let declarations = compiled.AssemblyContents.ImplementationFiles.Head.Declarations
  Assert.IsNotEmpty( declarations )
  let declaration = declarations.Head
  match declaration with
  | _ -> Assert.Fail(sprintf "OTHER: %A" declarations)
*)

[<Test>]
let ``simple "let a=123;;a+1" compile works`` () =
  let compiled = Library.main [|"--code";"let a=123;;a+1"|]
  Assert.NotNull(compiled)
  Assert.IsNotEmpty(compiled)
  print "123Add1" compiled