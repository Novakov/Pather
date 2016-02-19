module Pather.ExpressionEvaluator

open Grammar
open System
open Microsoft.FSharp.Reflection

type ValueResolver = string -> string -> obj option
type FunctionCaller = string * obj list -> obj option
type FunctionWrapper = obj list -> obj

type Visitor(value: ValueResolver, call: FunctionCaller) = class
    inherit PatherParserBaseVisitor<obj>()    
    
    member private this.FunctionCall (ctx: PatherParser.FunctionCallContext) (args: obj list) =
        match call (ctx.Name, args) with
        | Some(v) -> v

    override this.VisitBinaryExpression ctx = 
        match ctx.Operator with
        | BinaryOperator.Plus -> 
            let left = this.Visit(ctx.left)
            let right = this.Visit(ctx.right)

            match left with
            | :? double as l ->
                match right with
                | :? double as r -> box (l + r)
                | _ -> box (left.ToString() + right.ToString())
            | _ -> box (left.ToString() + right.ToString())
        | BinaryOperator.Pipe ->
            let value = this.Visit(ctx.left)
            let funcCall = ctx.right :?> PatherParser.FunctionCallContext

            let args = funcCall.Arguments |> Seq.map this.Visit |> Seq.toList

            this.FunctionCall funcCall (List.append args [value])
           
    override this.VisitNumberConstant ctx = box ctx.Value
    override this.VisitStringConstant ctx = box ctx.Value
    override this.VisitParenthesesExpression ctx = this.Visit(ctx.expression())
    override this.VisitValueReferenceExpression ctx =         
        match value ctx.Scope ctx.ValueName with
        | Some(v) -> v

    override this.VisitFunctionCall ctx =
        let args = ctx.Arguments |> Seq.map this.Visit |> Seq.toList
        this.FunctionCall ctx args

end

let wrap (f: 'a -> 'r): FunctionWrapper =
    let (argType, resType) = FSharpType.GetFunctionElements (f.GetType())
    let funcType = FSharpType.MakeFunctionType (argType, resType) 
    let invoke = funcType.GetMethod("Invoke")

    let makeParam (list: obj list) = 
        match list.Length with
        | 0 -> [| box () |]
        | 1 -> [| list.Head |]
        | _ ->
            let types = list |> Seq.map (fun i->i.GetType())|> Seq.toArray
            let tupleType = FSharpType.MakeTupleType types
            [| FSharpValue.MakeTuple (Seq.toArray list, tupleType) |]

    fun args -> invoke.Invoke(f, makeParam args)

let exposeFunctions (funs: (string * FunctionWrapper) seq) : FunctionCaller = 
    let map = Map.ofSeq funs

    fun (n, a) -> 
        match map.TryFind n with
        | Some(f) -> Some(f a)
        | None -> None

let evalute (root : PatherParser.ExpressionContext) (value: ValueResolver) (call: FunctionCaller) = 
    let visitor = new Visitor(value, call)
    visitor.Visit(root)

    