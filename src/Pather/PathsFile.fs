module Pather.PathsFile

open FParsec

type Group = { Name: string; Paths: PathSet.PathSet }

type ParseResult = 
    | Success of Group: Map<string, Group>
    | Failure of Error: string 
    override x.ToString() =
        match x with
        | Success(_) -> "Success"
        | Failure(msg) -> "Failure: " + msg

let parse input =
    let padding  = many (skipAnyOf [' '; '\t'])  |>> ignore

    let name = identifier (new IdentifierOptions())

    let groupHeader = pstring "group" >>. padding >>. name .>> padding .>> pstring "of" .>> padding .>> newline

    let groupEnd = pstring "end" .>> padding .>> newline >>% ()

    let notWhitespace (c:char) = match c with
                                 | ' ' | '\t' | '\n'  -> false
                                 | _ -> true
                                 

    let emptyLine = followedByNewline >>% None
    let nonEmptyLine = many1Satisfy notWhitespace .>> padding .>> followedByNewline

    let lineWithPath = nonEmptyLine |>> (fun i -> Some(new PathName(i.Trim())))

    let groupItem = ((lineWithPath) <|> (emptyLine))

    let groupItemWrapper = padding >>. groupItem .>> newline

    let group = optional spaces >>. groupHeader .>>. manyTill groupItemWrapper groupEnd .>> optional spaces
                   |>> (fun (groupName, paths) -> 
                        { 
                            Group.Name = groupName; 
                            Paths = paths |> Seq.filter (fun i->i.IsSome) |> Seq.map (fun i -> i.Value) |> PathSet.fromSeq
                        })                   

    let parser = many group .>> eof
                    |>> (Seq.map (fun i-> (i.Name, i)) >> Map.ofSeq)

    match run parser input with
     | ParserResult.Success(result, _, _) -> ParseResult.Success(result)
     | ParserResult.Failure(msg, _, _) -> ParseResult.Failure(msg)