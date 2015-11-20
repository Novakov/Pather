module Pather.Commands.Preview

open System.IO
open Pather.PathsFile
open Pather

let execute (args: Pather.Cli.PreviewArgs) =
    let input = File.ReadAllText args.File
    let groups = match parse input with
                 | Success(g) -> g
                 | Failure(m) -> failwith m

    groups |> Map.iter (fun k g ->
        printf "Group: %s:\n" k
        g.Paths |> PathSet.toSeq |> Seq.iter (fun p -> printf "\t'%s'\n" (p.Normalize().Path))
    )
    ()

     