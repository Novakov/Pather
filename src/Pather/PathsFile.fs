module Pather.PathsFile

open FParsec

type Group = { Name: string; Paths: PathSet.PathSet }

type ParseResult = 
    | Success of Group: Group list
    | Failure of Error: string 
    override x.ToString() =
        match x with
        | Success(_) -> "Success"
        | Failure(msg) -> "Failure: " + msg

let parse input =
    let padding  = many (skipAnyOf [' '; '\t'])  |>> ignore

    let name = identifier (new IdentifierOptions())

    let groupHeader = pstring "group" >>. padding >>. name .>> padding .>> pstring "of" .>> padding .>> newline

    let groupEnd = pstring "end" .>> padding .>> newline

    let groupItem = restOfLine false .>> newline 
                        |>> (fun p -> new PathName(p.Trim()))

    let group = optional spaces >>. groupHeader .>>. manyTill groupItem groupEnd .>> optional spaces
                   |>> (fun (groupName, paths) -> { Group.Name = groupName; Paths =  PathSet.fromSeq paths })

    let parser = many group .>> eof

    match run parser input with
     | ParserResult.Success(result, _, _) -> ParseResult.Success(result)
     | ParserResult.Failure(msg, _, _) -> ParseResult.Failure(msg)