module FunctionCallerTests

open Xunit
open FsUnit.Xunit
open FSharp.Reflection
open FSharp.Quotations

let call (arg: obj list) (f: Pather.ExpressionEvaluator.FunctionWrapper) =
    f arg

[<Fact>]
let FuncWithThreeParameters () = 
    Pather.ExpressionEvaluator.wrap (fun (a,b,c) -> sprintf "Args=[%d,%s,%d]" a b c)
    |> call [1; "a"; 3]
    |> should equal "Args=[1,a,3]"

[<Fact>]
let FuncWithSingleParameter () = 
    Pather.ExpressionEvaluator.wrap (fun a -> sprintf "Args=[%d]" a)
    |> call [1]
    |> should equal "Args=[1]"


[<Fact>]
let FuncWithNoParameters () = 
    Pather.ExpressionEvaluator.wrap (fun () -> sprintf "Args=[]" )
    |> call []
    |> should equal "Args=[]"