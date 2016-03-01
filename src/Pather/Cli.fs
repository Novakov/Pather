module Pather.Cli

open CommandLine
open System.Reflection

type A =
    | ExecuteCommand of Args: obj    
    | Error of Text: CommandLine.Text.HelpText

let private (|HelpRequested|_|) (x: ParserResult<obj>) = 
    match x with
    | :? Parsed<obj> -> None
    | :? NotParsed<obj> as p ->
        p.Errors |> Seq.tryFind (fun e -> e :? HelpRequestedError || e :? HelpVerbRequestedError)

let private parser = 
    new CommandLine.Parser(fun settings ->
                            settings.IgnoreUnknownArguments <- false
                        )

let parse (args: string[]) =
    let argTypes = Assembly.GetExecutingAssembly().GetTypes()
                    |> Seq.filter (fun x -> not (isNull (x.GetCustomAttribute(typeof<VerbAttribute>))))
                    |> Array.ofSeq


    let result = parser.ParseArguments(args, argTypes)
    match result with    
    | :? NotParsed<obj> as p -> Error(CommandLine.Text.HelpText.AutoBuild(p))
    | :? Parsed<obj> as p -> ExecuteCommand(p.Value)
