module ParsingTests

open Xunit
open Pather
open FsUnit.Xunit
open Pather.PathsFile

let success = NHamcrest.CustomMatcher<obj>("Parse successful", 
                fun x -> match x with
                         | :? ParseResult as y -> match y with
                                                   | Success(_) -> true
                                                   | Failure(_) -> false
                         | _ -> false
    )

let failure = NHamcrest.CustomMatcher<obj>("Parse failure", 
                fun x -> match x with
                         | :? ParseResult as y -> match y with
                                                   | Success(_) -> false
                                                   | Failure(_) -> true
                         | _ -> false
    )

let groups (pr:ParseResult) = 
    match pr with
    | Success(g) -> g
    | Failure(_) -> []

let group  (i:int) (pr:ParseResult) =
    (pr |> groups).Item(i)

[<Fact>]
let ``Input with two groups`` () =
    let input = @"


group my_group of
mg1.1
    mg1.2
end


group my_group2 of
mg2.1
mg2.2               
end
"

    let result = parse input 
    
    result |> should be success

    result |> groups |> should haveLength 2

    (result |> group 0).Name |> should equal "my_group"
    (result |> group 0).Paths |> PathSet.toSeq |> should equal [ new PathName("mg1.1"); new PathName("mg1.2") ]
        
    (result |> group 1).Name |> should equal "my_group2"
    (result |> group 1).Paths |> PathSet.toSeq |> should equal [ new PathName("mg2.1"); new PathName("mg2.2") ]

[<Fact>]
let ``Empty group is valid`` () =
    let input = @"
group my_group of
end
"

    let result = parse input
    result |> should be success
    match result with
    | Failure(msg) -> failwithf "Unexpected parse fail %s" msg
    | Success(groups)  -> ()

[<Fact>]
let ``Spaces after group header are valid`` ()=
    @"
group my_group of   
end     
" |> parse |> should be success

[<Fact>]
let ``Group header cannot span across multiple lines`` () =
    @"
group my_group
of
end    
" |> parse |> should be failure

[<Fact>]
let ``Group name must not contain spaces`` () =
    @"
group my group of   
end     
" |> parse |> should be failure