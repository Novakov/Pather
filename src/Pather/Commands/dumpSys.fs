module Pather.Commands.DumpSys

open Microsoft.Win32
open System.IO

open CommandLine

open Pather

[<Verb("dump-sys")>]
type Args = {
    [<Option('f', "file", Required = true)>]File: string
}

let execute (args: Args) =
    let group name set =
        seq {
            yield sprintf "group %s of" name
            yield! (set |> PathSet.toSeq |> Seq.map (fun p-> sprintf "\t%s" p.Path))
            yield "end"
        }

    [
        SystemSettings.getSystemPath () |> group "system" 
        SystemSettings.getUserPath ()   |> group "user" 
    ]
    |> Seq.concat
    |> Array.ofSeq
    |> fun lines -> File.WriteAllLines(args.File, lines)
        

    ()