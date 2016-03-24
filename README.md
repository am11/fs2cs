# fs2cs

Simple F# |> C# transpiler shamelessly built on the [Fable F# to JavaScript Compiler](https://github.com/fsprojects/Fable) shoulders 

* can translate simple F# .fsx files into C# .cs
* can translate F# .fsproj files to set of C# .cs files
* includes simple fs2csLib library to make C# more JavaScript-ish
* uses `dynamic` type everywhere to mimic JavaScript’s type system 

In order to build run 

    > build.cmd // on windows    
    $ ./build.sh  // on unix
    

## Build Status

Mono | .NET
---- | ----
[![Mono CI Build Status](https://img.shields.io/travis/fsprojects/ProjectScaffold/master.svg)](https://travis-ci.org/fsprojects/ProjectScaffold) | [![.NET Build Status](https://img.shields.io/appveyor/ci/fsgit/ProjectScaffold/master.svg)](https://ci.appveyor.com/project/fsgit/projectscaffold)

## Maintainer(s)

- [@davidpodhola](https://github.com/davidpodhola)
