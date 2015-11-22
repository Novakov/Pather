module Pather.Cli

open CommandLine
open System.Reflection

let parse (args: string[]) =
    let argTypes = Assembly.GetExecutingAssembly().GetTypes()
                    |> Seq.filter (fun x -> not (isNull (x.GetCustomAttribute(typeof<VerbAttribute>))))
                    |> Array.ofSeq

    let parser = new CommandLine.Parser(fun settings ->
                                            settings.IgnoreUnknownArguments <- false
                                        )

    let result = parser.ParseArguments(args, argTypes)
    match result with
    | :? NotParsed<obj> as p -> None
    | :? Parsed<obj> as p -> Some(p.Value)
