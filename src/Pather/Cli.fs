module Pather.Cli

open CommandLine

[<Verb("config-sys")>]
type ConfigSysArgs = { [<Option("file")>]File: string }

[<Verb("dump-sys")>]
type DumpSysArgs = { [<Option("file")>]File: string}

[<Verb("preview")>]
type PreviewArgs = { [<Option('f', "file")>]File: string }

let parse (args: string[]) =
    let argTypes = [|
        typeof<ConfigSysArgs>
        typeof<DumpSysArgs>
        typeof<PreviewArgs>
    |]
    let result = CommandLine.Parser.Default.ParseArguments(args, argTypes)
    match result with
    | :? NotParsed<obj> as p -> None
    | :? Parsed<obj> as p -> Some(p.Value)
