module Pather.Cli

open CommandLine

[<Verb("config-sys")>]
type ConfigSysArgs = { [<Option("file")>]File: string }

[<Verb("dump-sys")>]
type DumpSysArgs = { [<Option("file")>]File: string}

[<Verb("preview")>]
type PreviewArgs = { 
    [<Option('f', "file", Required = true)>]File: string 
}

[<Verb("run")>]
type RunArgs = {
    [<Option('f', "file", Required = true)>]File: string;
    [<Option('g', "group", Default  = "default")>]Group: string;
    [<Option('m', "mode", Default = "append")>]Mode: string;
    [<Value(0, HelpText = "command to run", Required = true)>]Command: string;
    [<Value(1, HelpText = "command args")>]Args: string seq
}

let parse (args: string[]) =
    let argTypes = [|
        typeof<ConfigSysArgs>
        typeof<DumpSysArgs>
        typeof<PreviewArgs>
        typeof<RunArgs>
    |]

    let parser = new CommandLine.Parser(fun settings ->
                                            settings.IgnoreUnknownArguments <- false                                                
                                        )

    let result = parser.ParseArguments(args, argTypes)
    match result with
    | :? NotParsed<obj> as p -> None
    | :? Parsed<obj> as p -> Some(p.Value)
