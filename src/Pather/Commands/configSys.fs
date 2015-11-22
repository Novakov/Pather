module Pather.Commands.ConfigSys

open System.IO
open CommandLine
open Pather

type Part = 
    | System = 1
    | User = 2
    | All = 3

[<Verb("config-sys")>]
type Args = {
    [<Option('f', "file", Required = true)>]File: string
    [<Option('p', "part", Default = Part.All)>]Part: Part
    [<Option('s', "system-group", Default = "system")>]SystemGroup: string
    [<Option('S', "system-merge", Default = PathSet.MergeKind.Append)>]SystemMergeKind: PathSet.MergeKind
    [<Option('u', "user-group", Default = "user")>]UserGroup: string
    [<Option('U', "user-merge", Default = PathSet.MergeKind.Append)>]UserMergeKind: PathSet.MergeKind
}

let execute (args: Args) =
    let input = File.ReadAllText args.File
    let file = match PathsFile.parse input with
                | PathsFile.Success(f) -> f
                | PathsFile.Failure(m) -> failwith m
    
    let setPart part group kind getter setter =
        if args.Part.HasFlag(part) then
            let inputSet = file.[group].Paths
            let existingSet = getter ()
            let finalSet = existingSet |> PathSet.merge inputSet kind
            
            setter finalSet
              
        ()

    setPart Part.System args.SystemGroup args.SystemMergeKind SystemSettings.getSystemPath SystemSettings.setSystemPath
    setPart Part.User args.UserGroup args.UserMergeKind SystemSettings.getUserPath SystemSettings.setUserPath

