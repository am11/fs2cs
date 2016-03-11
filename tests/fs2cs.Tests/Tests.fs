module fs2cs.Tests

open fs2cs
open NUnit.Framework

[<Test>]
let ``hello returns 42`` () =
  let result = Library.hello 42
  printfn "%i" result
  Assert.AreEqual(42,result)

[<Test>]
let ``empty () compile works`` () =
  let result = Library.main [|"--code";"()"|]
  printfn "%A" result
  Assert.NotNull(result)
