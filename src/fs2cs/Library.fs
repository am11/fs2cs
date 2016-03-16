namespace fs2cs

/// Documentation for my library
///
/// ## Example
///
///     let h = Library.hello 1
///     printfn "%d" h
///
module Library = 
  
  open System
  open System.IO
  open System.Reflection
  open Microsoft.FSharp.Compiler
  open Microsoft.FSharp.Compiler.Ast
  open Microsoft.FSharp.Compiler.SourceCodeServices

  let parse com checker projCode fileMask =
    try
        Fable.Main.parseFSharpProject com checker projCode
        |> Fable.FSharp2Fable.Compiler.transformFiles com fileMask
    with ex ->
        failwith ex.Message

  let compile com checker projCode fileMask =
    parse com checker projCode fileMask
    |> Fable2CSharp.Compiler.transformFiles com

  let main argv operation =
    let opts =
        Fable.Main.readOptions argv
        |> function
            | opts when opts.code <> null ->
                { opts with projFile = Path.ChangeExtension(Path.GetTempFileName(), "fsx") }
            | opts -> opts
    let plugins = Fable.Main.loadPlugins opts
    let com = { new Fable.ICompiler with
                member __.Options = opts
                member __.Plugins = plugins }
    let checker = FSharpChecker.Create(keepAssemblyContents=true)
    operation com checker (Option.ofObj opts.code) None
