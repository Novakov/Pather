module PathNameTests

open Xunit
open FsUnit.Xunit
open FsCheck
open Pather

[<FactAttribute>]
let ``Equality is case insensitive`` () =
    let a = new PathName(@"C:\Path1")
    let b = new PathName(@"C:\path1")

    a.Equals(b) |> should equal true

[<Fact>]
let ``Only equal paths should be equal`` () =
    let a = new PathName(@"C:\Path1")
    let b = new PathName(@"C:\Path1")
    let c = new PathName(@"C:\Path2")

    a.Equals(b) |> should equal true
    a.Equals(c) |> should equal false

[<Fact>]
let ``Equality is based on normalized paths`` () =
    let a = new PathName(@"C:\Path1\.\..\Path1")
    let b = new PathName(@"C:\Path1\.\Path2\..\.\.\")

    a.Equals(b) |> should equal true

[<Theory>]
[<InlineData(@"C:\My\Path")>]
[<InlineData(@"C:\My\Path\")>]
[<InlineData(@"C:/My/Path")>]
[<InlineData(@"C:/My\Path/")>]
[<InlineData(@"C:\My\Path\SubPath\..")>]
let ``Normalized path``(input)  =
    let path = new PathName(input)
    let normalized = path.Normalize()

    normalized.Path |> should equal "C:\My\Path"
