module PathSetTests

open Xunit
open FsUnit.Xunit
open Pather

[<Fact>]
let ``Contains added paths`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")

    let set =  [path1; path2] |> PathSet.fromSeq
    
    set |> PathSet.toSeq  |> should contain path1
    set |> PathSet.toSeq  |> should contain path2


[<Fact>]
let ``Add path`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")

    let initialSet = [path1] |> PathSet.fromSeq

    let finalSet = initialSet |> PathSet.add path2

    finalSet |> PathSet.toSeq  |> should contain path1
    finalSet |> PathSet.toSeq  |> should contain path2

[<Fact>]
let ``Insert path`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")
    let path3 = new PathName(@"C:\Path3")

    let set = [path1; path2] |> PathSet.fromSeq |> PathSet.insert 1 path3

    set |> PathSet.toSeq  |> should contain path3

[<Fact>]
let ``Order of paths in maintained`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")
    let path3 = new PathName(@"C:\Path3")
    let path4 = new PathName(@"C:\Path4")

    let set = [path1; path3] |> PathSet.fromSeq |> PathSet.add path4 |> PathSet.insert 2 path2
    
    set |> PathSet.toSeq |> should equal [path1; path3; path2; path4]

[<Fact>]
let ``Adding path that is already in set does nothing`` () =
     let path1 = new PathName(@"C:\Path1")
     [path1]
     |> PathSet.fromSeq
     |> PathSet.add path1
     |> PathSet.toSeq 
     |> should equal [path1]

[<Fact>]
let ``Does not contain path that is not in set`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")

    let set = [path1] |> PathSet.fromSeq
    set.Contains path2 |> should equal false
    
[<Fact>]
let ``Equals with itself`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")

    let set = [path1; path2] |> PathSet.fromSeq

    set.Equals(set) |> should equal true

[<Fact>]
let ``Equals with set of the same paths`` () =
    let path1 = new PathName(@"C:\Path1")
    let path21 = new PathName(@"C:\Path2")
    let path22 = new PathName(@"C:\Path2\")

    let set1 = [path1; path21] |> PathSet.fromSeq
    let set2 = [path1; path22] |> PathSet.fromSeq

    set1.Equals(set2) |> should equal true

[<Fact>]
let ``Different sets are not equal`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")
    let path3 = new PathName(@"C:\Path3")

    let set1 = [path1; path2] |> PathSet.fromSeq
    let set2 = [path1; path3] |> PathSet.fromSeq

    set1.Equals(set2) |> should equal false

[<Fact>]
let ``[Merge] TakeFirst should take first set`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")
    let path3 = new PathName(@"C:\Path3")
    let path4 = new PathName(@"C:\Path4")

    let set1 = [path1; path2] |> PathSet.fromSeq
    let set2 = [path3; path4] |> PathSet.fromSeq

    set1 |> PathSet.merge set2 PathSet.Leave |> should equal set1

[<Fact>]
let ``[Merge] TakeSecond should take second set`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")
    let path3 = new PathName(@"C:\Path3")
    let path4 = new PathName(@"C:\Path4")

    let set1 = [path1; path2] |> PathSet.fromSeq
    let set2 = [path3; path4] |> PathSet.fromSeq

    set1 |> PathSet.merge set2 PathSet.Replace |> should equal set2

[<Fact>]
let ``[Merge] Append should create set with all items from first, then unique from second`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")
    let path3 = new PathName(@"C:\Path3")

    let set1 = [path1; path2] |> PathSet.fromSeq
    let set2 = [path2; path3] |> PathSet.fromSeq
    let expected = [path1; path2; path3] |> PathSet.fromSeq

    set1 |> PathSet.merge set2 PathSet.Append |> should equal expected

[<Fact>]
let ``[Merge] Prepend should create set with all items from second, then unique from first`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")
    let path3 = new PathName(@"C:\Path3")

    let set1 = [path1; path2] |> PathSet.fromSeq
    let set2 = [path2; path3] |> PathSet.fromSeq
    let expected = [path2; path3; path1] |> PathSet.fromSeq

    set1 |> PathSet.merge set2 PathSet.Prepend |> should equal expected


[<Fact>]
let ``FromEnvVar should split by semicolon`` () =
    let path1 = new PathName(@"C:\Path1")
    let path2 = new PathName(@"C:\Path2")
    let path3 = new PathName(@"C:\Path3")

    let expected = [path1;path2;path3] |> PathSet.fromSeq

    "C:\Path1;C:\Path2;C:\Path3" |> PathSet.fromEnvVar |> should equal expected