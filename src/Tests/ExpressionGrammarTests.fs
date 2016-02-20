module ExpressionGrammarTests

open Xunit
open Xunit.Abstractions
open Grammar
open Antlr4.Runtime
open System.Linq
open FsUnit.Xunit
open Antlr4.Runtime.Tree
open Parsing

let cast<'t when 't: not struct> (o: obj) = 
    o |> should be ofExactType<'t>
    o :?> 't

let paren (ctx:IParseTree) = (cast<PatherParser.ParenthesesExpressionContext> ctx).expression()

let binary (ctx: IParseTree) = cast<PatherParser.BinaryExpressionContext> ctx    

let left (ctx: PatherParser.BinaryExpressionContext) = ctx.left
let right (ctx: PatherParser.BinaryExpressionContext) = ctx.right
let op (ctx: PatherParser.BinaryExpressionContext) = ctx.op.Text

let str (ctx: IParseTree) = (cast<PatherParser.StringConstantContext> ctx).Value

let num (ctx: IParseTree) = (cast<PatherParser.NumberConstantContext> ctx).Value

let valueRef (ctx: IParseTree) = cast<PatherParser.ValueReferenceExpressionContext> ctx

let valueScope (ctx: PatherParser.ValueReferenceExpressionContext) = ctx.Scope
let valueName (ctx: PatherParser.ValueReferenceExpressionContext) = ctx.ValueName

let funcCall (ctx: IParseTree) = (cast<PatherParser.FunctionCallContext> ctx)

let simple (ctx: IParseTree) = (cast<PatherParser.WrappedSimpleExpressionContext> ctx).simpleExpression()

type Expression =
        | Number of Value: double
        | String of Value: string
        | Paren of Inner: Expression
        | Binary of Left: Expression * Op: string * Right: Expression        
        | Plus of Left: Expression * Right: Expression        
        | Pipe of Left: Expression * Right: Expression        
        | ValueRef of Scope: string * Name: string
        | FuncCall of Name: string * Args: Expression list  
        | Simple of Expr: Expression     

let rec assertTree (actual: IParseTree) (expected: Expression) =
    match expected with
    | Simple(e) -> assertTree (actual |> simple) e
    | Number(v) -> actual |> num |> should equal v
    | String(v) -> actual |> str |> should equal v
    | Paren(e) -> assertTree (actual |> paren) e
    | Plus(l, r) -> assertTree actual (Binary(l, "+", r))
    | Pipe(l, r) -> assertTree actual (Binary(l, "|", r))
    | Binary(l, o, r) ->
        assertTree (actual |> binary |> left) l
        assertTree (actual |> binary |> right) r
        actual |> binary |> op |> should equal o
    | ValueRef(s, n) ->
        actual |> valueRef |> valueScope |> should equal s
        actual |> valueRef |> valueName |> should equal n
    | FuncCall(n, a) ->
        let call = actual |> funcCall
        call.Name |> should equal n
        call.Arguments.Length |> should equal a.Length
        call.Arguments |> Seq.zip a |> Seq.iter (fun (x, y) -> assertTree y x)
  
type Test(out: ITestOutputHelper) =
    member private this.Parse (input: string) =
        input 
        |> createLexer 
        |> lexMode PatherLexer.EXPRESSION
        |> allTokens
        |> skipHiddenTokens
        |> Seq.iter (fun t -> 
            let typeName = PatherLexer.DefaultVocabulary.GetSymbolicName(t.Type)            

            sprintf "(%d, %d) %s -> %s" t.Line t.Column t.Text typeName
            |> out.WriteLine  
        )
       
        out.WriteLine("--------------------")
        
        let tree =
            input 
            |> createLexer 
            |> lexMode PatherLexer.EXPRESSION
            |> createParser
            |> errorListener (new DiagnosticErrorListener())
            |> errorListener (new xUnitErrorListener(out))
            |> (fun p -> p.expression())
            |> stringTree out.WriteLine    

        out.WriteLine("--------------------")

        tree


    [<Fact>]
    member __.StringConstant () =
        let root = __.Parse "'test string'"

        let expected = Simple(String("test string"))

        assertTree root expected

    [<Theory>]
    [<InlineData("42.12", 42.12)>]
    [<InlineData("-42.12", -42.12)>]
    [<InlineData("-42", -42)>]
    member __.NumericConstant (input: string) (value: double) =
        let root = __.Parse input    

        let expected = Simple(Number(value))

        assertTree root expected

    [<Fact>]
    member __.PlusOperator () =
        let root = __.Parse "'aaa' + 'bbb'"
       
        let expected = 
            Plus(
                Simple(String("aaa")),
                Simple(String("bbb"))
            )

        assertTree root expected

    [<Fact>]
    member __.Parentheses () =
        let root = __.Parse "'ccc' + ('aaa' + 'bbb')"

        let expected = 
            Plus(
                Simple(String("ccc")),
                Simple(
                    Paren(
                        Plus(Simple(String("aaa")), Simple(String("bbb")))
                    )
                )
            )

        assertTree root expected  

    [<Fact>]
    member __.BinaryOpAssoc () =
        let root = __.Parse "'a' + 'b' + 'c'"        

        let expected = 
            Plus(
                Plus(Simple(String("a")), Simple(String("b"))),
                Simple(String("c"))
            )

        assertTree root expected

    [<Fact>]
    member __.ValueReference () =
        let root = __.Parse "env:COMPUTERNAME"

        let expected = Simple(ValueRef("env", "COMPUTERNAME"))

        assertTree root expected

    [<Fact>]
    member __.ValueReferenceWithSpaces () =
        let root = __.Parse "reg:'HKLM:\Software\Microsoft\Windows NT\CurrentVersion'"

        let expected = Simple(ValueRef("reg", "HKLM:\Software\Microsoft\Windows NT\CurrentVersion"))

        assertTree root expected

    [<Fact>]
    member __.FunctionCallWithoutArguments () =
        let root = __.Parse "some_function"

        let expected = FuncCall("some_function", [])

        assertTree root expected

    [<Fact>]
    member __.ArglessFunctionCallsInComplexExpression () =
        let root = __.Parse "func1  + (2.0 + func2 + func3)"

        let expected =            
                Plus(
                    FuncCall("func1", []),
                    Simple(
                        Paren(
                            Plus(
                                Plus(Simple(Number(2.0)), FuncCall("func2", [])),
                                FuncCall("func3", [])
                            )
                        )
                    )
                )            

        assertTree root expected

    [<Fact>]
    member __.FunctionWithArgs () =
        let root = __.Parse "func 'arg1' 1.0 'arg2'"

        let expected = FuncCall("func", [ String("arg1"); Number(1.0); String("arg2") ])

        assertTree root expected  

    [<Fact>]
    member __.FunctionCallHasHigherPriorityThanBinaryExpression () =
        let root = __.Parse "func 1.3 + 4"

        let expected = 
            Plus(
                FuncCall("func", [ Number(1.3) ]),
                Simple(Number(4.0))
            )

        assertTree root expected

    [<Fact>]
    member __.PipingFunctions () =
        let root = __.Parse("func1 | func2 'arg1'")

        let expected = 
            Pipe(
                FuncCall("func1", []),
                FuncCall("func2", [String("arg1")])
            )

        assertTree root expected

    [<Fact>]
    member __.ExpressionsPiping () =
        let root = __.Parse("func1 + 2 | func2 'arg1' + 3 | func3")

        let expected = 
            Pipe(
                Plus(
                    Pipe(
                        Plus(FuncCall("func1", []), Simple(Number(2.0))),
                        FuncCall("func2", [String("arg1")])
                    ),
                    Simple(Number(3.0))
                ),
                FuncCall("func3", [])
            )

        assertTree root expected