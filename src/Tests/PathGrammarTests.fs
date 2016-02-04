module PathGrammarTests

open System.Linq
open Xunit
open Xunit.Abstractions
open Antlr4.Runtime
open Grammar

type Tests(out: ITestOutputHelper) =
    
    static member Paths () =
        seq {
            yield [| @"C:\" |]
            yield [| @"d:\Path1" |]            
            yield [| @"d:\Path1\" |]            
            yield [| @"d:\Path1\Path2" |]            
            yield [| @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools" |]            
            yield [| @"\\server\share" |]
            yield [| @"\\server.domain.com\share\share2" |]
        }

    [<TheoryAttribute()>]
    [<MemberDataAttribute("Paths")>]
    member __.Tokens (path: string) =
        let inputStream = new AntlrInputStream(path)

        let lexer = new PathLexer(inputStream)
       
        lexer.GetAllTokens()
        |> Seq.filter (fun t -> t.Channel <> PathLexer.Hidden)
        |> Seq.iter (fun t ->
            let typeName = PathLexer.tokenNames.ElementAtOrDefault(t.Type)

            sprintf "(%d, %d) %s -> %s" t.Line t.Column t.Text typeName
            |> out.WriteLine            
        )
        
        ()

    [<TheoryAttribute()>]
    [<MemberDataAttribute("Paths")>]
    member __.ParseTree (path: string) =
        let inputStream = new AntlrInputStream(path)

        let lexer = new PathLexer(inputStream)

        let tokenStream = new BufferedTokenStream(lexer)

        let parser = new PathParser(tokenStream)

        let root = parser.root()

        let tree = root.ToStringTree(parser)

        out.WriteLine(tree)

        ()