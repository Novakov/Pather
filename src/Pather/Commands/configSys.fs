module Pather.Commands.ConfigSys

open System.IO
open CommandLine
open Pather

type Part = 
    | System = 1
    | User = 2
    | All = 3

[<Verb("config-sys", HelpText = "Load path sets into system PATH")>]
type Args = {
    [<Option('f', "file", Required = true, HelpText = "Input file")>]File: string
    [<Option('p', "part", Default = Part.All, HelpText = "Parts that should be loaded")>]Part: Part
    [<Option('s', "system-group", Default = "system", HelpText = "Name of group for system PATH")>]SystemGroup: string
    [<Option('S', "system-merge", Default = PathSet.MergeKind.Append, HelpText = "Merge mode for system PATH")>]SystemMergeKind: PathSet.MergeKind
    [<Option('u', "user-group", Default = "user", HelpText = "Name of group for user PATH")>]UserGroup: string
    [<Option('U', "user-merge", Default = PathSet.MergeKind.Append, HelpText = "Merge mode for user PATH")>]UserMergeKind: PathSet.MergeKind
}

let execute (args: Args) =
    let input = File.ReadAllText args.File
    let file = match PathsFile.parse input with
                | PathsFile.Success(f) -> f
                | PathsFile.Failure(m) -> failwith m
    
    let setPart part group kind getter setter =
        if args.Part.HasFlag(part) then
            let inputSet = file.[group]
            let existingSet = getter ()
            let finalSet = existingSet |> PathSet.merge inputSet kind
            
            setter finalSet
              
        ()

    setPart Part.System args.SystemGroup args.SystemMergeKind SystemSettings.getSystemPath SystemSettings.setSystemPath
    setPart Part.User args.UserGroup args.UserMergeKind SystemSettings.getUserPath SystemSettings.setUserPath

