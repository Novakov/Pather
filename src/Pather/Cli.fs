module Pather.Cli

open CommandLine
open System.Reflection

type A =
    | ExecuteCommand of Args: obj
    | Error of Errors: string seq

let private formatError (e:Error) = 
    match e with 
    | :? NamedError as n -> sprintf "%s: %s" n.NameInfo.NameText (n.Tag.ToString())
    | _ -> e.Tag.ToString()

let parse (args: string[]) =
    let argTypes = Assembly.GetExecutingAssembly().GetTypes()
                    |> Seq.filter (fun x -> not (isNull (x.GetCustomAttribute(typeof<VerbAttribute>))))
                    |> Array.ofSeq

    let parser = new CommandLine.Parser(fun settings ->
                                            settings.IgnoreUnknownArguments <- false
                                        )

    let result = parser.ParseArguments(args, argTypes)
    match result with
    | :? NotParsed<obj> as p -> Error(p.Errors |> Seq.map formatError)
    | :? Parsed<obj> as p -> ExecuteCommand(p.Value)
