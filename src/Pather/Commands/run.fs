module Pather.Commands.Run

open System
open System.IO
open System.Diagnostics
open Pather
open CommandLine

[<Verb("run")>]
type Args = {
    [<Option('f', "file", Required = true)>]File: string;
    [<Option('g', "group", Default  = "default")>]Group: string;
    [<Option('m', "mode", Default = "append")>]Mode: string;
    [<Value(0, HelpText = "command to run", Required = true)>]Command: string;
    [<Value(1, HelpText = "command args")>]Args: string seq
}


let execute (args: Args) = 
    let input = File.ReadAllText args.File
    let file = match PathsFile.parse input with
                | PathsFile.Success(f) -> f
                | PathsFile.Failure(m) -> failwith m

    let group = file.Item(args.Group)

    let parentSet = Environment.GetEnvironmentVariable("PATH") |> PathSet.fromEnvVar

    let mergeKind = match args.Mode with
                     | "append" -> PathSet.Append
                     | "prepend" -> PathSet.Prepend
                     | "replace" -> PathSet.Replace
                     | "leave" -> PathSet.Leave
                     | _ -> PathSet.Append

    let finalSet = parentSet |> PathSet.merge group.Paths mergeKind 

    let startInfo = new ProcessStartInfo(args.Command, args.Args|> String.concat " ")
    startInfo.EnvironmentVariables.["PATH"] <- finalSet |> PathSet.toEnvVar
    startInfo.UseShellExecute <- false

    let proc = Process.Start(startInfo)
    proc.WaitForExit()