module Pather.Commands.Run

open System
open System.IO
open System.Diagnostics
open Pather
open CommandLine

[<Verb("run", HelpText = "Run process with given path set")>]
type Args = {
    [<Option('f', "file", Required = true, HelpText = "Input file")>]File: string;
    [<Option('g', "group", Default  = "default", HelpText = "Name of group to use")>]Group: string;
    [<Option('m', "mode", Default = PathSet.MergeKind.Append, HelpText = "Pathset merge mode")>]Mode: PathSet.MergeKind;
    [<Value(0, HelpText = "Command to run", Required = true)>]Command: string;
    [<Value(1, HelpText = "Command args")>]Args: string seq
}


let execute (args: Args) = 
    let input = File.ReadAllText args.File
    let file = match PathsFile.parse input with
                | PathsFile.Success(f) -> f
                | PathsFile.Failure(m) -> failwith m

    let group = file.Item(args.Group)

    let parentSet = Environment.GetEnvironmentVariable("PATH") |> PathSet.fromEnvVar

    let finalSet = parentSet |> PathSet.merge group args.Mode 

    let startInfo = new ProcessStartInfo(args.Command, args.Args|> String.concat " ")
    startInfo.EnvironmentVariables.["PATH"] <- finalSet |> PathSet.toEnvVar
    startInfo.UseShellExecute <- false

    let proc = Process.Start(startInfo)
    proc.WaitForExit()