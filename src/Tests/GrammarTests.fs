module GrammarTests

open Antlr4.Runtime

open Xunit
open Grammar
open System.Linq

let buildParser (input:string) =    
    let inputStream = new Antlr4.Runtime.AntlrInputStream(input)

    let lexer = new PathsFileLexer(inputStream)

    let tokenStream = new Antlr4.Runtime.BufferedTokenStream(lexer)

    let parser = new PathsFileParser(tokenStream)    
    
    (lexer, parser)

type ErrorListener(out: Xunit.Abstractions.ITestOutputHelper) = 
    interface IAntlrErrorListener<int> with
        member self.SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e) =
            out.WriteLine("({0}, {1}): {2}", line, charPositionInLine, msg)
        
let input = @"
group Default of     
    C:\   
    C:\path
    C:\path\
    C:\path\path2
end

group Default2 of     
    C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A  
end
"

type Tests(out: Xunit.Abstractions.ITestOutputHelper) =        
    [<Fact>]
    member __.ParseWholeFile () =

        let (lexer, parser) = buildParser input
        
        lexer.AddErrorListener(ErrorListener(out))

        let root = parser.compileUnit()                            
        
        let paths = root.group() 
                    |> Seq.map (fun i -> i.line())
                    |> Seq.concat
                    |> Seq.map (fun i -> " - " + i.text)
                    |> Seq.toList        

        out.WriteLine("Paths:")
        paths 
            |> Seq.iter (fun i -> out.WriteLine(i))

        out.WriteLine("")        

        let tree = root.ToStringTree(parser)
                    .Replace(@"\r\n", "\r\n")

        out.WriteLine(tree)        

        ()

    [<Fact>]
    member __.Lexer() =
        let (lexer, parser) = buildParser input
        
        lexer.AddErrorListener(ErrorListener(out))

        lexer.GetAllTokens()
        |> Seq.filter (fun t -> t.Channel <> PathsFileLexer.Hidden)
        |> Seq.iter (fun t -> out.WriteLine("{2},{3} {0} -> {1}", t.Text, PathsFileLexer.tokenNames.ElementAtOrDefault(t.Type), t.Line, t.Column))

