module Parsing

open Xunit.Abstractions
open Antlr4.Runtime
open Grammar
open Antlr4.Runtime.Tree

type xUnitErrorListener(out: ITestOutputHelper) = 
    interface IAntlrErrorListener<IToken> with        
        member this.SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e) =
            let t = PatherParser.DefaultVocabulary.GetDisplayName(offendingSymbol.Type)
            out.WriteLine("({0}, {1}): {2} -> {3}", line, charPositionInLine, t, msg)
            ()

let createLexer (input:string) =
     let charStream = new AntlrInputStream(input)
        
     new PatherLexer(charStream)

let lexMode mode (lexer: PatherLexer) =
    lexer.Mode(mode)
    lexer

let skipHiddenTokens (tokens: IToken seq) =
    tokens |> Seq.filter (fun t -> t.Channel <> PatherLexer.Hidden)

let allTokens (lexer: PatherLexer) =
    lexer.GetAllTokens()

let createParser (lexer: PatherLexer) =
    let tokenStream = new CommonTokenStream(lexer)
    
    new PatherParser(tokenStream)    
    

let errorListener listener (parser: PatherParser) =
    parser.AddErrorListener(listener)

    parser

let stringTree (out: string -> unit) (ctx: ParserRuleContext)  = 
    let stringTree = ctx.ToStringTree(new PatherParser(null)).Replace("\\r", "\r").Replace("\\n", "\n")

    out stringTree

    ctx

let walk listener (ctx: ParserRuleContext) =
    let walker = new ParseTreeWalker()

    walker.Walk(listener, ctx)

    ctx