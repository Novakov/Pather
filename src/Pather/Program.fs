// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open Pather

[<EntryPoint>]
let main argv = 
    let commandArgs = Cli.parse argv

    match commandArgs with
    | None -> ()
    | Some(command) ->
        let moduleType = command.GetType().DeclaringType

        let executeMethod = moduleType.GetMethod("execute")

        executeMethod.Invoke(null, [|command|]) |> ignore

    0