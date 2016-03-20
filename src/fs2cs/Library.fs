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
  open Fable.Main
  open Fable

  let compile (com: ICompiler) checker projOpts fileMask =
      let projOpts =
          getProjectOptions com checker projOpts fileMask
      projOpts
      |> parseFSharpProject com checker
      |> FSharp2Fable.Compiler.transformFiles com fileMask
      |> Seq.map (Fable2CSharp.Compiler.transformFiles com)
      |> Seq.concat

  
  let main argv =
    let opts =
        readOptions argv
        |> function
            | opts when opts.code <> null ->
                { opts with projFile = Path.ChangeExtension(Path.GetTempFileName(), "fsx") }
            | opts -> opts
    let com = makeCompiler (loadPlugins opts) opts
    let checker = FSharpChecker.Create(keepAssemblyContents=true)
    compile com checker None None
