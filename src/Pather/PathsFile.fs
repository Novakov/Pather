module Pather.PathsFile

open Antlr4.Runtime
open Antlr4.Runtime.Tree
open Grammar
open System.Text


type ParseResult = 
    | Success of Group : Map<string, PathSet.PathSet>
    | Failure of Error : string
    override x.ToString() = 
        match x with
        | Success(_) -> "Success"
        | Failure(msg) -> "Failure: " + msg

type Collector() = 
    inherit PatherParserBaseVisitor<obj>()
       
    override this.VisitNamePart ctx =
        ctx.PATH_NAME_FRAGMENT().GetText() |> box

    override this.VisitRelativePath ctx =
        let seps = ctx.SEP()
        let names = ctx.name()        
        let rec merge xs ys = 
            seq {
                let xh = Seq.tryHead xs
                let yh = Seq.tryHead ys                

                match (xh, yh) with
                | (None, None) -> ()
                | _ -> 
                    yield (xh, yh)
                    yield! merge (Seq.tail xs) (Seq.tail ys)

                yield! []
            }

        let l = merge seps names |> Seq.toList

        let result =
            merge seps names
            |> Seq.fold (fun r (s, n) ->
                let sep = match s with
                          | None -> ""
                          | Some(x) -> x.GetText()

                let name = match n with
                           | None -> ""
                           | Some(x) -> (this.Visit x) :?> string

                r + sep + name
            ) ""

        box (result.ToString())

    override this.VisitLocalPath ctx = 
        let drive = ctx.DRIVE().GetText()
        let relPath = (this.Visit(ctx.relativePath()) :?> string)        
        drive + relPath |> box

    override this.VisitGroup ctx = 
        ctx.path()
        |> Seq.map (fun p -> (this.Visit p) :?> string)
        |> Seq.map PathName.FromString
        |> PathSet.fromSeq
        |> box

    member this.Collect(ctx : PatherParser.RootContext) = 
        ctx.group()
        |> Seq.map (fun g -> (g.Id, this.Visit g :?> PathSet.PathSet))
        |> Map.ofSeq

let parse (input : string) = 
    let charStream = new AntlrInputStream(input)
    let lexer = new PatherLexer(charStream)
    lexer.Mode(PatherLexer.PATHS_FILE)
    //
    //    lexer.GetAllTokens()
    //    |> Seq.filter (fun t->t.Channel <> PatherLexer.Hidden)
    //    |> Seq.iter( fun t ->
    //        let tt = PatherLexer.DefaultVocabulary.GetDisplayName t.Type
    //        printfn "(%d,%d) %s -> %s" t.Line t.Column t.Text tt
    //    )
    let tokenStream = new CommonTokenStream(lexer)
    let parser = new PatherParser(tokenStream)
    let root = parser.root()
    let collector = new Collector()
    ParseResult.Success(collector.Collect root)
