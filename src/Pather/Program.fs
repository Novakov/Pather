// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open Pather

[<EntryPoint>]
let main argv = 
    let commandArgs = Cli.parse argv

    match commandArgs with
    | Cli.Error(errors) -> 
        errors |> Seq.iter (printf "Error: %s\n")

    | Cli.ExecuteCommand(command) ->
        let moduleType = command.GetType().DeclaringType

        let executeMethod = moduleType.GetMethod("execute")

        executeMethod.Invoke(null, [|command|]) |> ignore

    0