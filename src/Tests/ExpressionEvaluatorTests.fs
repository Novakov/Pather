module ExpressionEvaluatorTests

open Xunit
open Xunit.Abstractions
open FsUnit.Xunit
open Antlr4.Runtime
open Grammar
open Pather

type Tests(out: ITestOutputHelper) =
    member private __.Parse (input:string) =
        let charStream = new AntlrInputStream(input)
        
        let lexer = new PatherLexer(charStream)

        let tokenStream = new CommonTokenStream(lexer)

        let parser = new PatherParser(tokenStream)

        lexer.Mode(PatherLexer.EXPRESSION)
        parser.expression()

    member private __.Evaluate (root: PatherParser.ExpressionContext) = 
        let caller = ExpressionEvaluator.exposeFunctions [
                        ("args3", ExpressionEvaluator.wrap __.Arg3)
                        ("argless", ExpressionEvaluator.wrap __.ArglessFunc)
                        ("pipeFunc", ExpressionEvaluator.wrap __.PipeFunc)
                      ]
        ExpressionEvaluator.evalute root __.ResolveValue caller

    member private __.ResolveValue (scope:string) (name:string) = 
        match scope with
        | "env" -> match name with
                    | "COMPUTERNAME" -> Some(box "MyMachine")

    member private __.ArglessFunc () = "ArglessResult"

    member private __.Arg3 (a,b,c) = sprintf "Args=[%.0f,%.0f,%.0f]" a b c

    member private __.PipeFunc (a,b,c) = sprintf "Args=[%.0f,%.0f,%.0f]" a b c  

    member private __.Test input expected = 
        __.Parse input |> __.Evaluate |> should equal expected

    [<Theory>]
    [<InlineData("1 + 2", 3.0)>]
    [<InlineData("'a' + 'b'", "ab")>]
    [<InlineData("1 + 'b'", "1b")>]
    [<InlineData("'a' + 1", "a1")>]
    member __.PlusOperator (input: string) (expected: obj) = __.Test input expected

    [<Theory>]
    [<InlineData("(1 + 2) + 3", 6.0)>]
    [<InlineData("(1 + 2) + '3'", "33")>]
    member __.ParenthesesExpression (input: string, expected: obj) = __.Test input expected      

    [<Fact>]
    member __.ValueReference () = __.Test "env:COMPUTERNAME" "MyMachine"             

    [<Fact>]
    member __.ArglessFunctionCall () = __.Test "argless" "ArglessResult"

    [<Fact>]
    member __.ThreeArgsFunctionCall () = __.Test  "args3 1 2 3" "Args=[1,2,3]"

    [<Theory>]
    [<InlineData("1 + 3 | pipeFunc 1 2", "Args=[1,2,4]")>]
    [<InlineData("1 + 3 | pipeFunc 1 2 + '3'", "Args=[1,2,4]3")>]
    member __.PipeExpressionToFunctionCall (input: string) (expected: string) = __.Test input expected
   
