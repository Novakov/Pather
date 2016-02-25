module ParsingErrorsTests

open Grammar
open Parsing
open Xunit
open Xunit.Abstractions
open Antlr4.Runtime
open FsUnit.Xunit
open Extensions

type Error = { Token: IToken; Exception: RecognitionException }

type ErrorListener(out : ITestOutputHelper) =
    let mutable list: Error list = []

    member this.List with get() = list

    interface IAntlrErrorListener<IToken> with
        member this.SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e) =
            let error = { Token = offendingSymbol; Exception = e }
            list <- error::list
            ()

type RightSideOfPipeOperatorIsNotFunctionCall(recognizer, input, ctx) = 
    inherit RecognitionException("Right side of pipe operator must be function call", recognizer, input, ctx)


type ErrorChecker(out : ITestOutputHelper, parser: Parser) =
    inherit PatherParserBaseListener()

    override this.EnterBinaryExpression ctx = 
        if ctx.Operator = BinaryOperator.Pipe then
            match ctx.right with
            | :? PatherParser.FunctionCallContext -> ()
            | ctx ->
                let id = ctx.Start

                let e = RightSideOfPipeOperatorIsNotFunctionCall(parser, parser.InputStream, ctx)

                parser.ErrorListenerDispatch.SyntaxError(parser, id, id.Line, id.Column, e.Message, e)            





type Tests(out : ITestOutputHelper) = 
    member val errors = ErrorListener(out)


    member private this.Parse input = 
        let parser = 
            input
            |> createLexer
            |> lexMode PatherLexer.EXPRESSION
            |> createParser        
            |> errorListener this.errors

        let root = parser.expression()
        walk (ErrorChecker(out, parser)) root
    
    [<Fact>]
    member __.``Right side of pipe operator must be function call``() = 
        let input = "1 | 2"
        let tree = __.Parse input

        __.errors.List |> should haveLength 1
        __.errors.List.Head 
        |> should match' (fun a -> 
            a.Token.Text |> should equal "2"
            a.Exception |> should be instanceOfType<RightSideOfPipeOperatorIsNotFunctionCall>
        )
