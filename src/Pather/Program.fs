// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open Pather

[<EntryPoint>]
let main argv = 
    let commandArgs = Cli.parse argv

    match commandArgs with
    | None -> failwith "Invalid command\n"
    | Some(command) ->
        match command with 
        | :? Cli.PreviewArgs as a -> Pather.Commands.Preview.execute a


    0