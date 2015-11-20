module Pather.Commands.Run

open System.IO
open System.Diagnostics
open Pather

let execute (args: Cli.RunArgs) = 
    let input = File.ReadAllText args.File
    let file = match PathsFile.parse input with
                | PathsFile.Success(f) -> f
                | PathsFile.Failure(m) -> failwith m

    let group = file.Item(args.Group)

    let startInfo = new ProcessStartInfo(args.Command, args.Args|> String.concat " ")
    startInfo.EnvironmentVariables.["PATH"] <- group.Paths |> PathSet.toEnvVar
    startInfo.UseShellExecute <- false

    let proc = Process.Start(startInfo)
    proc.WaitForExit()